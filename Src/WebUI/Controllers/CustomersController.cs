using System.Data;
using Microsoft.EntityFrameworkCore;
using Northwind.Domain.Entities;
using Northwind.Persistence;
using Northwind.WebUI.Graftcode;

namespace Northwind.WebUI.Controllers;

public class CustomersController
{
    private const string ConnectionStringEnv = "NORTHWIND_DB_CONNECTION_STRING";
    private const int CustomerIdLength = 5;
    private const int AddressMaxLength = 60;
    private const int CityMaxLength = 15;
    private const int CompanyNameMaxLength = 40;
    private const int ContactNameMaxLength = 30;
    private const int ContactTitleMaxLength = 30;
    private const int CountryMaxLength = 15;
    private const int FaxMaxLength = 24;
    private const int PhoneMaxLength = 24;
    private const int PostalCodeMaxLength = 10;
    private const int RegionMaxLength = 15;

    public static string[] GetCustomers() => GraftAsync.Run(GetCustomersAsync);

    public static string GetCustomerById(string id) =>
        GraftAsync.Run(() => GetCustomerByIdAsync(id));

    public static string RegisterCustomer(
        string id,
        string companyName,
        string contactName,
        string contactTitle,
        string address,
        string city,
        string region,
        string postalCode,
        string country,
        string phone,
        string fax
    ) => GraftAsync.Run(
        () =>
            RegisterCustomerAsync(
                id,
                companyName,
                contactName,
                contactTitle,
                address,
                city,
                region,
                postalCode,
                country,
                phone,
                fax
            )
    );

    public static string UpdateCustomer(
        string id,
        string companyName,
        string contactName,
        string contactTitle,
        string address,
        string city,
        string region,
        string postalCode,
        string country,
        string phone,
        string fax
    ) => GraftAsync.Run(
        () =>
            UpdateCustomerAsync(
                id,
                companyName,
                contactName,
                contactTitle,
                address,
                city,
                region,
                postalCode,
                country,
                phone,
                fax
            )
    );

    public static string DeleteCustomer(string id) => GraftAsync.Run(() => DeleteCustomerAsync(id));

    private static async Task<string[]> GetCustomersAsync()
    {
        await using var context = CreateContext();
        var customers = await context
            .Customers.OrderBy(customer => customer.CustomerId)
            .ToArrayAsync()
            .ConfigureAwait(false);
        return customers.Select(FormatCustomer).ToArray();
    }

    private static async Task<string> GetCustomerByIdAsync(string id)
    {
        var customerId = RequireFixedLengthValue(id, nameof(id), CustomerIdLength);
        await using var context = CreateContext();
        var customer = await context.Customers.FindAsync(customerId).ConfigureAwait(false);
        return customer == null ? string.Empty : FormatCustomer(customer);
    }

    private static async Task<string> RegisterCustomerAsync(
        string id,
        string companyName,
        string contactName,
        string contactTitle,
        string address,
        string city,
        string region,
        string postalCode,
        string country,
        string phone,
        string fax
    )
    {
        var customerId = RequireFixedLengthValue(id, nameof(id), CustomerIdLength);
        var company = RequireValueWithMaxLength(companyName, nameof(companyName), CompanyNameMaxLength);
        var contact = OptionalValueWithMaxLength(contactName, nameof(contactName), ContactNameMaxLength);
        var title = OptionalValueWithMaxLength(contactTitle, nameof(contactTitle), ContactTitleMaxLength);
        var street = OptionalValueWithMaxLength(address, nameof(address), AddressMaxLength);
        var town = OptionalValueWithMaxLength(city, nameof(city), CityMaxLength);
        var province = OptionalValueWithMaxLength(region, nameof(region), RegionMaxLength);
        var postcode = RequireValueWithMaxLength(postalCode, nameof(postalCode), PostalCodeMaxLength);
        var nation = OptionalValueWithMaxLength(country, nameof(country), CountryMaxLength);
        var phoneNumber = OptionalValueWithMaxLength(phone, nameof(phone), PhoneMaxLength);
        var faxNumber = OptionalValueWithMaxLength(fax, nameof(fax), FaxMaxLength);

        await using var context = CreateContext();
        var exists = await context
            .Customers.AnyAsync(customer => customer.CustomerId == customerId)
            .ConfigureAwait(false);
        if (exists)
        {
            throw new InvalidOperationException($"Customer '{customerId}' already exists.");
        }

        var entity = new Customer
        {
            CustomerId = customerId,
            CompanyName = company,
            ContactName = contact,
            ContactTitle = title,
            Address = street,
            City = town,
            Region = province,
            PostalCode = postcode,
            Country = nation,
            Phone = phoneNumber,
            Fax = faxNumber
        };

        context.Customers.Add(entity);
        await context.SaveChangesAsync(default).ConfigureAwait(false);
        return FormatCustomer(entity);
    }

    private static async Task<string> UpdateCustomerAsync(
        string id,
        string companyName,
        string contactName,
        string contactTitle,
        string address,
        string city,
        string region,
        string postalCode,
        string country,
        string phone,
        string fax
    )
    {
        var customerId = RequireValueWithMaxLength(id, nameof(id), CustomerIdLength);
        var company = RequireValueWithMaxLength(companyName, nameof(companyName), CompanyNameMaxLength);
        var contact = OptionalValueWithMaxLength(contactName, nameof(contactName), ContactNameMaxLength);
        var title = OptionalValueWithMaxLength(contactTitle, nameof(contactTitle), ContactTitleMaxLength);
        var street = OptionalValueWithMaxLength(address, nameof(address), AddressMaxLength);
        var town = OptionalValueWithMaxLength(city, nameof(city), CityMaxLength);
        var province = OptionalValueWithMaxLength(region, nameof(region), RegionMaxLength);
        var postcode = OptionalValueWithMaxLength(postalCode, nameof(postalCode), PostalCodeMaxLength);
        var nation = OptionalValueWithMaxLength(country, nameof(country), CountryMaxLength);
        var phoneNumber = RequireValueWithMaxLength(phone, nameof(phone), PhoneMaxLength);
        var faxNumber = RequireValueWithMaxLength(fax, nameof(fax), FaxMaxLength);

        EnsureAustralianPostcode(nation, postcode);
        EnsureQueenslandLandline(nation, postcode, phoneNumber, faxNumber);

        await using var context = CreateContext();
        var entity = await context
            .Customers.SingleOrDefaultAsync(customer => customer.CustomerId == customerId)
            .ConfigureAwait(false);
        if (entity == null)
        {
            throw new InvalidOperationException($"Customer '{customerId}' was not found.");
        }

        entity.CompanyName = company;
        entity.ContactName = contact;
        entity.ContactTitle = title;
        entity.Address = street;
        entity.City = town;
        entity.Region = province;
        entity.PostalCode = postcode;
        entity.Country = nation;
        entity.Phone = phoneNumber;
        entity.Fax = faxNumber;

        await context.SaveChangesAsync(default).ConfigureAwait(false);
        return FormatCustomer(entity);
    }

    private static async Task<string> DeleteCustomerAsync(string id)
    {
        var customerId = RequireFixedLengthValue(id, nameof(id), CustomerIdLength);
        await using var context = CreateContext();
        var entity = await context.Customers.FindAsync(customerId).ConfigureAwait(false);

        if (entity == null)
        {
            throw new InvalidOperationException($"Entity \"Customer\" ({customerId}) was not found.");
        }

        if (await HasOrdersTableAsync(context).ConfigureAwait(false))
        {
            var hasOrders = await context
                .Orders.AnyAsync(order => order.CustomerId == entity.CustomerId)
                .ConfigureAwait(false);
            if (hasOrders)
            {
                throw new InvalidOperationException(
                    $"Deletion of entity \"Customer\" ({customerId}) failed. There are existing orders associated with this customer."
                );
            }
        }

        context.Customers.Remove(entity);
        await context.SaveChangesAsync(default).ConfigureAwait(false);
        return customerId;
    }

    private static NorthwindDbContext CreateContext()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnv);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Missing required environment variable '{ConnectionStringEnv}'."
            );
        }

        var options = new DbContextOptionsBuilder<NorthwindDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new NorthwindDbContext(options);
    }

    private static string FormatCustomer(Customer customer) =>
        string.Join(
            "|",
            customer.CustomerId ?? string.Empty,
            customer.CompanyName ?? string.Empty,
            customer.ContactName ?? string.Empty,
            customer.ContactTitle ?? string.Empty,
            customer.Address ?? string.Empty,
            customer.City ?? string.Empty,
            customer.Region ?? string.Empty,
            customer.PostalCode ?? string.Empty,
            customer.Country ?? string.Empty,
            customer.Phone ?? string.Empty,
            customer.Fax ?? string.Empty
        );

    private static async Task<bool> HasOrdersTableAsync(NorthwindDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;
        if (shouldCloseConnection)
        {
            await connection.OpenAsync().ConfigureAwait(false);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Orders'";
            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
            return result != null && result != DBNull.Value;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    private static void EnsureAustralianPostcode(string country, string postalCode)
    {
        if (
            string.Equals(country, "Australia", StringComparison.Ordinal)
            && (postalCode.Length != 4 || !postalCode.All(char.IsDigit))
        )
        {
            throw new InvalidOperationException("Australian Postcodes have 4 digits");
        }
    }

    private static void EnsureQueenslandLandline(
        string country,
        string postalCode,
        string phone,
        string fax
    )
    {
        if (
            string.Equals(country, "Australia", StringComparison.Ordinal)
            && postalCode.StartsWith("4", StringComparison.Ordinal)
            && !phone.StartsWith("07", StringComparison.Ordinal)
            && !fax.StartsWith("07", StringComparison.Ordinal)
        )
        {
            throw new InvalidOperationException("Customers in QLD require at least one QLD landline.");
        }
    }

    private static string OptionalValueWithMaxLength(string? value, string name, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        EnsureMaxLength(normalized, name, maxLength);
        return normalized;
    }

    private static string RequireFixedLengthValue(string? value, string name, int expectedLength)
    {
        var normalized = RequireValue(value, name);
        if (normalized.Length != expectedLength)
        {
            throw new InvalidOperationException(
                $"{name} must be exactly {expectedLength} characters."
            );
        }

        return normalized;
    }

    private static string RequireValueWithMaxLength(string? value, string name, int maxLength)
    {
        var normalized = RequireValue(value, name);
        EnsureMaxLength(normalized, name, maxLength);
        return normalized;
    }

    private static void EnsureMaxLength(string value, string name, int maxLength)
    {
        if (value.Length > maxLength)
        {
            throw new InvalidOperationException(
                $"{name} must be {maxLength} characters or fewer."
            );
        }
    }

    private static string RequireValue(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} is required.");
        }

        return value.Trim();
    }
}