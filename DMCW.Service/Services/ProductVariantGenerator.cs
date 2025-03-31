using DMCW.Repository.Data.Entities.product;

public static class ProductVariantGenerator
{
    private static int variantCounter = 0;

    public static List<ProductVariant> GenerateProductVariants(List<VariantOption> options, string? baseSku, decimal? basePrice, int? baseAvailableQuantity)
    {
        var variants = new List<ProductVariant>();
        var combinations = GenerateCombinations(options);

        foreach (var combination in combinations)
        {
            var variant = new ProductVariant
            {
                VariantId = ++variantCounter,
                SKU = $"{baseSku}-{string.Join("-", combination.Select(x => x.Value))}",
                Name = string.Join(", ", combination.Select(x => x.Value)),
                Price = basePrice,
                AvailableQuantity = baseAvailableQuantity,
                CommittedQuantity = 0,
                IsActive = true
            };

            variants.Add(variant);
        }

        return variants;
    }

    private static List<Dictionary<string, string>> GenerateCombinations(List<VariantOption> options)
    {
        var combinations = new List<Dictionary<string, string>>();

        void Recurse(int index, Dictionary<string, string> current)
        {
            if (index == options.Count)
            {
                combinations.Add(new Dictionary<string, string>(current));
                return;
            }

            var option = options[index];
            foreach (var value in option.Values)
            {
                current[option.Name] = value;
                Recurse(index + 1, current);
                current.Remove(option.Name);
            }
        }

        Recurse(0, new Dictionary<string, string>());
        return combinations;
    }
}