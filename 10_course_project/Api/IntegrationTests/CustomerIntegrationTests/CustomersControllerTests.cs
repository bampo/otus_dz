using System.Net.Http.Json;
using Customers.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Customers;

namespace CustomerIntegrationTests;

public class CustomersControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    protected async Task<CustomerDbContext> GetDbContext()
    {
        var scope = factory.Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<CustomerDbContext>();

        // Ensure the database is clean before each test
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        return db;
    }

    [Fact]
    public async Task GetCustomers_ReturnsAllCustomers()
    {
        // Arrange
        var db = await GetDbContext();

        // Add test data
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Salt = "test_salt_1",
                PasswordHash = "test_hash_1"
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Salt = "test_salt_2",
                PasswordHash = "test_hash_2"
            }
        };
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();

        // Act - use first customer's ID for auth
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", customers[0].Id.ToString());
        var response = await client.GetAsync("/api/customers");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
        Assert.NotNull(result);
        Assert.Equal(customers.Count, result.Count);

        var orderedCustomers = customers.OrderBy(c => c.Id).ToList();
        var orderedResult = result.OrderBy(c => c.Id).ToList();
            
        for (int i = 0; i < orderedCustomers.Count; i++)
        {
            Assert.Equal(orderedCustomers[i].Id, orderedResult[i].Id);
            Assert.Equal(orderedCustomers[i].FirstName, orderedResult[i].FirstName);
            Assert.Equal(orderedCustomers[i].LastName, orderedResult[i].LastName);
            Assert.Equal(orderedCustomers[i].Email, orderedResult[i].Email);
        }
    }

    [Fact]
    public async Task GetCustomer_ReturnsCorrectCustomer()
    {
        // Arrange
        var db = await GetDbContext();

        // Add test data
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Salt = "test_salt_3",
            PasswordHash = "test_hash_3"
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        // Act
        var client = factory.CreateClient();
        // Add authentication header for the customer
        client.DefaultRequestHeaders.Add("X-User-Id", customer.Id.ToString());
        var response = await client.GetAsync($"/api/customers/{customer.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal(customer.FirstName, result.FirstName);
        Assert.Equal(customer.LastName, result.LastName);
        Assert.Equal(customer.Email, result.Email);
    }

    [Fact]
    public async Task GetCustomer_ReturnsNotFound()
    {
        // Arrange
        var db = await GetDbContext();

        // Act
        var client = factory.CreateClient();
        // Add authentication header for a non-existent customer
        var randomId = Guid.NewGuid().ToString();
        client.DefaultRequestHeaders.Add("X-User-Id", randomId);
        var response = await client.GetAsync($"/api/customers/{Guid.Parse(randomId)}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Customer not found", content);
    }
}