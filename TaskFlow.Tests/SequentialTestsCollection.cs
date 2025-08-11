using Xunit;

[CollectionDefinition("SequentialTests", DisableParallelization = true)]
public class SequentialTestsCollection : ICollectionFixture<object>;