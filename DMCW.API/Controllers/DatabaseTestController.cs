using DMCW.Repository.Data.DataService;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace DMCW.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        private readonly MongoDBContext _mongoContext;
        private readonly OracleDBContext _oracleContext;
        private readonly ILogger<DatabaseTestController> _logger;

        public DatabaseTestController(
            MongoDBContext mongoContext,
            OracleDBContext oracleContext,
            ILogger<DatabaseTestController> logger)
        {
            _mongoContext = mongoContext;
            _oracleContext = oracleContext;
            _logger = logger;
        }

       
        [HttpGet("oracle-test")]
        public async Task<IActionResult> TestOracleConnection()
        {
            try
            {
                var connection = _oracleContext.CreateConnection();
                await connection.OpenAsync();
                
                // Get database version
                string version = connection.ServerVersion;
                
                // Try to execute a simple query
                using var command = new OracleCommand("SELECT 1 FROM DUAL", connection);
                var result = await command.ExecuteScalarAsync();
                
                await connection.CloseAsync();
                
                return Ok(new { 
                    success = true, 
                    message = "Oracle connection successful", 
                    version = version,
                    testQuery = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to Oracle");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Oracle connection failed: {ex.Message}" 
                });
            }
        }

        [HttpGet("table-info")]
        public async Task<IActionResult> GetOracleTableInfo(string tableName)
        {
            try
            {
                var query = $@"
                    SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, NULLABLE 
                    FROM ALL_TAB_COLUMNS 
                    WHERE TABLE_NAME = :tableName 
                    ORDER BY COLUMN_ID";
                
                var parameters = new OracleParameter[] {
                    new OracleParameter("tableName", tableName.ToUpper())
                };
                
                var result = await _oracleContext.ExecuteQueryAsync(query, parameters);
                
                return Ok(new {
                    success = true,
                    table = tableName.ToUpper(),
                    columns = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting Oracle table info for {tableName}");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Oracle query failed: {ex.Message}" 
                });
            }
        }
        
        [HttpPost("create-tables")]
        public async Task<IActionResult> CreateOracleTables()
        {
            var results = new Dictionary<string, object>();
            
            try
            {
                // Create Products table
                var productsTableSql = @"
                CREATE TABLE PRODUCTS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    TITLE VARCHAR2(255),
                    DESCRIPTION CLOB,
                    PRODUCT_TYPE NUMBER(2),
                    FEATURED_REVIEW_ID VARCHAR2(36),
                    AVERAGE_RATING NUMBER(3,2),
                    REVIEW_COUNT NUMBER(10),
                    CLIENT_ID VARCHAR2(36),
                    CREATED_AT TIMESTAMP,
                    UPDATED_AT TIMESTAMP,
                    DELETED_AT TIMESTAMP,
                    IS_DELETED NUMBER(1) DEFAULT 0
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(productsTableSql);
                    results.Add("PRODUCTS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("PRODUCTS", new { success = false, error = ex.Message });
                }
                
                // Create PRODUCT_IMAGES table (for ImgUrls and Images collections)
                var productImagesTableSql = @"
                CREATE TABLE PRODUCT_IMAGES (
                    ID VARCHAR2(36) PRIMARY KEY,
                    PRODUCT_ID VARCHAR2(36),
                    IMAGE_URL VARCHAR2(500),
                    IS_ORIGINAL NUMBER(1) DEFAULT 0,
                    SORT_ORDER NUMBER(5) DEFAULT 0,
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (PRODUCT_ID) REFERENCES PRODUCTS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(productImagesTableSql);
                    results.Add("PRODUCT_IMAGES", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("PRODUCT_IMAGES", new { success = false, error = ex.Message });
                }
                
                // Create PRODUCT_TAGS table (for Tags collection)
                var productTagsTableSql = @"
                CREATE TABLE PRODUCT_TAGS (
                    PRODUCT_ID VARCHAR2(36),
                    TAG_NAME VARCHAR2(100),
                    PRIMARY KEY (PRODUCT_ID, TAG_NAME),
                    FOREIGN KEY (PRODUCT_ID) REFERENCES PRODUCTS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(productTagsTableSql);
                    results.Add("PRODUCT_TAGS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("PRODUCT_TAGS", new { success = false, error = ex.Message });
                }
                
                // Create ProductVariants table
                var productVariantsTableSql = @"
                CREATE TABLE PRODUCT_VARIANTS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    PRODUCT_ID VARCHAR2(36),
                    VARIANT_ID NUMBER(10),
                    SKU VARCHAR2(100),
                    NAME VARCHAR2(255),
                    PRICE NUMBER(10,2),
                    AVAILABLE_QUANTITY NUMBER(10),
                    COMMITTED_QUANTITY NUMBER(10),
                    IS_ACTIVE NUMBER(1) DEFAULT 1,
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (PRODUCT_ID) REFERENCES PRODUCTS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(productVariantsTableSql);
                    results.Add("PRODUCT_VARIANTS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("PRODUCT_VARIANTS", new { success = false, error = ex.Message });
                }
                
                // Create VariantOptions table
                var variantOptionsTableSql = @"
                CREATE TABLE VARIANT_OPTIONS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    PRODUCT_ID VARCHAR2(36),
                    NAME VARCHAR2(100),
                    POSITION NUMBER(5),
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (PRODUCT_ID) REFERENCES PRODUCTS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(variantOptionsTableSql);
                    results.Add("VARIANT_OPTIONS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("VARIANT_OPTIONS", new { success = false, error = ex.Message });
                }
                
                // Create ProductReviews table
                var productReviewsTableSql = @"
                CREATE TABLE PRODUCT_REVIEWS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    PRODUCT_ID VARCHAR2(36),
                    REVIEWER_ID VARCHAR2(36),
                    REVIEWER_NAME VARCHAR2(255),
                    REVIEWER_PROFILE_PICTURE VARCHAR2(500),
                    RATING NUMBER(1),
                    COMMENT CLOB,
                    LIKES_COUNT NUMBER(10) DEFAULT 0,
                    IS_FEATURED NUMBER(1) DEFAULT 0,
                    IS_VERIFIED NUMBER(1) DEFAULT 0,
                    CREATED_AT TIMESTAMP,
                    UPDATED_AT TIMESTAMP,
                    IS_DELETED NUMBER(1) DEFAULT 0,
                    FOREIGN KEY (PRODUCT_ID) REFERENCES PRODUCTS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(productReviewsTableSql);
                    results.Add("PRODUCT_REVIEWS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("PRODUCT_REVIEWS", new { success = false, error = ex.Message });
                }
                
                // Create REVIEW_IMAGES table
                var reviewImagesTableSql = @"
                CREATE TABLE REVIEW_IMAGES (
                    ID VARCHAR2(36) PRIMARY KEY,
                    REVIEW_ID VARCHAR2(36),
                    IMAGE_URL VARCHAR2(500),
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (REVIEW_ID) REFERENCES PRODUCT_REVIEWS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(reviewImagesTableSql);
                    results.Add("REVIEW_IMAGES", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("REVIEW_IMAGES", new { success = false, error = ex.Message });
                }
                
                // Create Orders table
                var ordersTableSql = @"
                CREATE TABLE ORDERS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    NAME VARCHAR2(100),
                    FINANCIAL_STATUS VARCHAR2(50),
                    FULFILLMENT_STATUS VARCHAR2(50),
                    NOTE CLOB,
                    PHONE VARCHAR2(20),
                    SUBTOTAL_PRICE NUMBER(10,2),
                    TOTAL_LINE_ITEMS_PRICE NUMBER(10,2),
                    TOTAL_PRICE NUMBER(10,2),
                    TOTAL_SHIPPING_PRICE NUMBER(10,2),
                    TOTAL_DISCOUNT_PRICE NUMBER(10,2),
                    CLIENT_ID VARCHAR2(36),
                    CREATED_AT TIMESTAMP,
                    UPDATED_AT TIMESTAMP,
                    DELETED_AT TIMESTAMP,
                    IS_DELETED NUMBER(1) DEFAULT 0,
                    IS_CANCELLED NUMBER(1) DEFAULT 0
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(ordersTableSql);
                    results.Add("ORDERS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("ORDERS", new { success = false, error = ex.Message });
                }
                
                // Create Order Payment Methods table (for PaymentMethod array)
                var orderPaymentMethodsTableSql = @"
                CREATE TABLE ORDER_PAYMENT_METHODS (
                    ORDER_ID VARCHAR2(36),
                    PAYMENT_METHOD VARCHAR2(50),
                    PRIMARY KEY (ORDER_ID, PAYMENT_METHOD),
                    FOREIGN KEY (ORDER_ID) REFERENCES ORDERS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(orderPaymentMethodsTableSql);
                    results.Add("ORDER_PAYMENT_METHODS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("ORDER_PAYMENT_METHODS", new { success = false, error = ex.Message });
                }
                
                // Create LineItems table
                var lineItemsTableSql = @"
                CREATE TABLE LINE_ITEMS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    ORDER_ID VARCHAR2(36),
                    FULFILLABLE_QUANTITY NUMBER(10),
                    FULFILLMENT_STATUS VARCHAR2(50),
                    NAME VARCHAR2(255),
                    PRICE NUMBER(10,2),
                    PRODUCT_ID VARCHAR2(36),
                    QUANTITY NUMBER(10),
                    TITLE VARCHAR2(255),
                    VARIANT_TITLE VARCHAR2(255),
                    IMAGE_URL VARCHAR2(500),
                    VARIANT_ID NUMBER(10),
                    SELLER_ID VARCHAR2(36),
                    SELLER_NAME VARCHAR2(255),
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (ORDER_ID) REFERENCES ORDERS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(lineItemsTableSql);
                    results.Add("LINE_ITEMS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("LINE_ITEMS", new { success = false, error = ex.Message });
                }
                
                // Create ShippingAddress table
                var shippingAddressTableSql = @"
                CREATE TABLE SHIPPING_ADDRESSES (
                    ID VARCHAR2(36) PRIMARY KEY,
                    ORDER_ID VARCHAR2(36) UNIQUE,
                    FIRST_NAME VARCHAR2(100),
                    LAST_NAME VARCHAR2(100),
                    PHONE VARCHAR2(20),
                    ADDRESS1 VARCHAR2(255),
                    ADDRESS2 VARCHAR2(255),
                    CITY VARCHAR2(100),
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (ORDER_ID) REFERENCES ORDERS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(shippingAddressTableSql);
                    results.Add("SHIPPING_ADDRESSES", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("SHIPPING_ADDRESSES", new { success = false, error = ex.Message });
                }
                
                // Create PaymentInfo table
                var paymentInfoTableSql = @"
                CREATE TABLE PAYMENT_INFOS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    ORDER_ID VARCHAR2(36) UNIQUE,
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (ORDER_ID) REFERENCES ORDERS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(paymentInfoTableSql);
                    results.Add("PAYMENT_INFOS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("PAYMENT_INFOS", new { success = false, error = ex.Message });
                }
                
                // Create Payments table
                var paymentsTableSql = @"
                CREATE TABLE PAYMENTS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    PAYMENT_INFO_ID VARCHAR2(36),
                    PAYMENT_METHOD VARCHAR2(50),
                    AMOUNT NUMBER(10,2),
                    CREATED_AT TIMESTAMP,
                    UPDATED_AT TIMESTAMP,
                    FOREIGN KEY (PAYMENT_INFO_ID) REFERENCES PAYMENT_INFOS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(paymentsTableSql);
                    results.Add("PAYMENTS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("PAYMENTS", new { success = false, error = ex.Message });
                }
                
                // Create Customer table (CustomerInfo)
                var customerTableSql = @"
                CREATE TABLE CUSTOMERS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    ORDER_ID VARCHAR2(36) UNIQUE,
                    EMAIL VARCHAR2(255),
                    FIRST_NAME VARCHAR2(100),
                    LAST_NAME VARCHAR2(100),
                    PHONE VARCHAR2(20),
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (ORDER_ID) REFERENCES ORDERS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(customerTableSql);
                    results.Add("CUSTOMERS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("CUSTOMERS", new { success = false, error = ex.Message });
                }
                
                // Create TimeLineDetails table
                var timeLineTableSql = @"
                CREATE TABLE TIMELINE_DETAILS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    ORDER_ID VARCHAR2(36),
                    COMMENT CLOB,
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (ORDER_ID) REFERENCES ORDERS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(timeLineTableSql);
                    results.Add("TIMELINE_DETAILS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("TIMELINE_DETAILS", new { success = false, error = ex.Message });
                }
                
                // Create Timeline Images table
                var timelineImagesTableSql = @"
                CREATE TABLE TIMELINE_IMAGES (
                    ID VARCHAR2(36) PRIMARY KEY,
                    TIMELINE_ID VARCHAR2(36),
                    IMAGE_URL VARCHAR2(500),
                    IS_ORIGINAL NUMBER(1) DEFAULT 0,
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (TIMELINE_ID) REFERENCES TIMELINE_DETAILS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(timelineImagesTableSql);
                    results.Add("TIMELINE_IMAGES", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("TIMELINE_IMAGES", new { success = false, error = ex.Message });
                }
                
                // Create Order Tags table
                var orderTagsTableSql = @"
                CREATE TABLE ORDER_TAGS (
                    ORDER_ID VARCHAR2(36),
                    TAG_NAME VARCHAR2(100),
                    PRIMARY KEY (ORDER_ID, TAG_NAME),
                    FOREIGN KEY (ORDER_ID) REFERENCES ORDERS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(orderTagsTableSql);
                    results.Add("ORDER_TAGS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("ORDER_TAGS", new { success = false, error = ex.Message });
                }
                
                // Create Users table
                var usersTableSql = @"
                CREATE TABLE USERS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    CLIENT_ID VARCHAR2(36),
                    EMAIL VARCHAR2(255),
                    NAME VARCHAR2(255),
                    PHONE VARCHAR2(50),
                    PROFILE_PICTURE_URL VARCHAR2(500),
                    USER_ROLE VARCHAR2(20),
                    CREATED_AT TIMESTAMP,
                    UPDATED_AT TIMESTAMP,
                    LAST_LOGIN_DATE TIMESTAMP,
                    DELETED_AT TIMESTAMP,
                    IS_DELETED NUMBER(1) DEFAULT 0,
                    IS_ACTIVE NUMBER(1) DEFAULT 1
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(usersTableSql);
                    results.Add("USERS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("USERS", new { success = false, error = ex.Message });
                }
                
                // Create User Address table
                var addressTableSql = @"
                CREATE TABLE USER_ADDRESSES (
                    ID VARCHAR2(36) PRIMARY KEY,
                    USER_ID VARCHAR2(36) UNIQUE,
                    STREET VARCHAR2(255),
                    CITY VARCHAR2(100),
                    STATE VARCHAR2(100),
                    ZIP_CODE VARCHAR2(20),
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (USER_ID) REFERENCES USERS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(addressTableSql);
                    results.Add("USER_ADDRESSES", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("USER_ADDRESSES", new { success = false, error = ex.Message });
                }
                
                // Create Seller Profile table
                var sellerProfileTableSql = @"
                CREATE TABLE SELLER_PROFILES (
                    ID VARCHAR2(36) PRIMARY KEY,
                    USER_ID VARCHAR2(36) UNIQUE,
                    AVERAGE_RATING NUMBER(3,2),
                    IS_VERIFIED_SELLER NUMBER(1) DEFAULT 0,
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (USER_ID) REFERENCES USERS(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(sellerProfileTableSql);
                    results.Add("SELLER_PROFILES", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("SELLER_PROFILES", new { success = false, error = ex.Message });
                }
                
                // Create Store Details table
                var storeDetailsTableSql = @"
                CREATE TABLE STORE_DETAILS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    SELLER_PROFILE_ID VARCHAR2(36) UNIQUE,
                    STORE_NAME VARCHAR2(255),
                    STORE_DESCRIPTION CLOB,
                    CREATED_AT TIMESTAMP,
                    FOREIGN KEY (SELLER_PROFILE_ID) REFERENCES SELLER_PROFILES(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(storeDetailsTableSql);
                    results.Add("STORE_DETAILS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("STORE_DETAILS", new { success = false, error = ex.Message });
                }
                
                // Create Seller Reviews table
                var sellerReviewsTableSql = @"
                CREATE TABLE SELLER_REVIEWS (
                    ID VARCHAR2(36) PRIMARY KEY,
                    SELLER_PROFILE_ID VARCHAR2(36),
                    REVIEWER_ID VARCHAR2(36),
                    REVIEWER_NAME VARCHAR2(255),
                    REVIEWER_PROFILE_PICTURE VARCHAR2(500),
                    RATING NUMBER(1),
                    COMMENT CLOB,
                    LIKES_COUNT NUMBER(10) DEFAULT 0,
                    IS_FEATURED NUMBER(1) DEFAULT 0,
                    IS_VERIFIED NUMBER(1) DEFAULT 0,
                    CREATED_AT TIMESTAMP,
                    UPDATED_AT TIMESTAMP,
                    IS_DELETED NUMBER(1) DEFAULT 0,
                    FOREIGN KEY (SELLER_PROFILE_ID) REFERENCES SELLER_PROFILES(ID)
                )";
                
                try
                {
                    await _oracleContext.ExecuteNonQueryAsync(sellerReviewsTableSql);
                    results.Add("SELLER_REVIEWS", new { success = true });
                }
                catch (Exception ex)
                {
                    results.Add("SELLER_REVIEWS", new { success = false, error = ex.Message });
                }
                
                return Ok(new { 
                    results = results,
                    message = "Tables creation process completed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Oracle tables");
                return StatusCode(500, new { 
                    success = false, 
                    message = $"Oracle tables creation failed: {ex.Message}",
                    results = results
                });
            }
        }
    }
} 