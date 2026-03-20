using System;

namespace Telenor.Api.TestExecutionManagement.Import;

/// <summary>
/// Thrown when import cannot proceed due to missing or invalid configuration/credentials.
/// This exception is never caught by the resilient Try* wrappers — it always propagates to the controller.
/// </summary>
public class ImportConfigurationException(string message) : Exception(message);