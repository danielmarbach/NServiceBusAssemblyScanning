var endpointConfiguration = new EndpointConfiguration("AssemblyPerfHarness");
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.UseTransport<LearningTransport>();
endpointConfiguration.UsePersistence<LearningPersistence>();

Console.WriteLine("Hit <enter> to create the endpoint");
Console.ReadLine();
var createdEndpoint = await Endpoint.Create(endpointConfiguration);
Console.ReadLine();
