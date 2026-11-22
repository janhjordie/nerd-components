// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text.Json;

namespace TheNerdCollective.Cli.Mollie;

/// <summary>
/// A local-only template for creating a Mollie payment.
/// </summary>
public sealed record ProductTemplate(string Sku, string Name, string Amount, string Currency, string Description);

/// <summary>
/// Reads and writes product templates on the local machine; Mollie has no product catalog API.
/// </summary>
public sealed class ProductTemplateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    /// <summary>
    /// Initializes the store at the given path or in the current user's application-data directory.
    /// </summary>
    public ProductTemplateStore(string? filePath = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrWhiteSpace(applicationData))
            {
                throw new InvalidOperationException("Could not resolve the current user's application-data directory.");
            }

            filePath = Path.Combine(applicationData, "TheNerdCollective", "tnc-mollie", "product-templates.json");
        }

        _filePath = Path.GetFullPath(filePath);
    }

    /// <summary>
    /// Gets all locally stored templates.
    /// </summary>
    public async Task<IReadOnlyList<ProductTemplate>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
        return JsonSerializer.Deserialize<List<ProductTemplate>>(json, JsonOptions)
            ?? throw new InvalidDataException("The local Mollie product-template file is empty or invalid.");
    }

    /// <summary>
    /// Adds a local template. Existing SKUs are not overwritten.
    /// </summary>
    public async Task AddAsync(ProductTemplate template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);
        var normalized = NormalizeAndValidate(template);
        var templates = (await ListAsync(cancellationToken)).ToList();
        if (templates.Any(existing => string.Equals(existing.Sku, normalized.Sku, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A local template with SKU '{normalized.Sku}' already exists.");
        }

        templates.Add(normalized);
        await SaveAsync(templates, cancellationToken);
    }

    /// <summary>
    /// Removes a local template by SKU.
    /// </summary>
    public async Task RemoveAsync(string sku, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        var templates = (await ListAsync(cancellationToken)).ToList();
        var removed = templates.RemoveAll(template => string.Equals(template.Sku, sku, StringComparison.OrdinalIgnoreCase));
        if (removed == 0)
        {
            throw new InvalidOperationException($"No local template with SKU '{sku}' exists.");
        }

        await SaveAsync(templates, cancellationToken);
    }

    private async Task SaveAsync(List<ProductTemplate> templates, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath)
            ?? throw new InvalidOperationException("The local template path has no parent directory.");
        Directory.CreateDirectory(directory);

        var temporaryPath = $"{_filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            var json = JsonSerializer.Serialize(templates, JsonOptions);
            await File.WriteAllTextAsync(temporaryPath, json, cancellationToken);
            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static ProductTemplate NormalizeAndValidate(ProductTemplate template)
    {
        var sku = template.Sku.Trim();
        var name = template.Name.Trim();
        var amount = template.Amount.Trim();
        var currency = template.Currency.Trim().ToUpperInvariant();

        if (sku.Length == 0 || name.Length == 0)
        {
            throw new ArgumentException("A product-template SKU and name are required.", nameof(template));
        }

        if (!decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedAmount)
            || parsedAmount <= 0)
        {
            throw new ArgumentException("Amount must be a positive decimal using a dot as the decimal separator.", nameof(template));
        }

        if (currency.Length != 3 || !currency.All(char.IsAsciiLetter))
        {
            throw new ArgumentException("Currency must be a three-letter ISO currency code.", nameof(template));
        }

        var description = string.IsNullOrWhiteSpace(template.Description) ? name : template.Description.Trim();
        return new ProductTemplate(sku, name, amount, currency, description);
    }
}
