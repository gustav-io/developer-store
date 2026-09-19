using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Events.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Transport.InMem;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

/// <summary>
/// Configures the Rebus bus. The in-memory transport keeps the evaluation self-contained;
/// swapping to Azure Service Bus is <c>t.UseAzureServiceBus(connectionString, QueueName)</c>
/// with the Rebus.AzureServiceBus package (RabbitMQ, SQS, etc. are the same one-line change).
/// </summary>
public class MessagingModuleInitializer : IModuleInitializer
{
    private const string QueueName = "developer-store.sales";

    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AutoRegisterHandlersFromAssemblyOf<SaleEventsConsumer>();

        builder.Services.AddRebus(
            configure => configure
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), QueueName)),
            onCreated: async bus =>
            {
                await bus.Subscribe<SaleCreated>();
                await bus.Subscribe<SaleModified>();
                await bus.Subscribe<SaleCancelled>();
                await bus.Subscribe<ItemCancelled>();
            });
    }
}
