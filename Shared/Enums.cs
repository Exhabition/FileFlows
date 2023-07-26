﻿namespace FileFlows.Shared;

/// <summary>
/// A list of types of operating systems
/// </summary>
public enum OperatingSystemType
{
    /// <summary>
    /// Unknown operating system
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Windows operating system
    /// </summary>
    Windows = 1,
    /// <summary>
    /// Linux operating system
    /// </summary>
    Linux = 2,
    /// <summary>
    /// Mac/Apple operating system
    /// </summary>
    Mac = 3,
    /// <summary>
    /// A docker system
    /// </summary>
    Docker = 4,
    /// <summary>
    /// Free BSD
    /// </summary>
    FreeBsd = 5
}

/// <summary>
/// A list of architecture types
/// </summary>
public enum ArchitectureType
{
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// 32 bit x86
    /// </summary>
    x86 = 1,
    /// <summary>
    /// 64bit x86
    /// </summary>
    x64 = 2,
    /// <summary>
    /// ARM 32 bit
    /// </summary>
    Arm32 = 3,
    /// <summary> 
    /// ARM 64 bit
    /// </summary>
    Arm64 = 4
}

/// <summary>
/// A type of Flow
/// </summary>
public enum FlowType
{
    /// <summary>
    /// A standard flow
    /// </summary>
    Standard = 0,
    /// <summary>
    /// A special flow that is executed when a flow fails during execution
    /// </summary>
    Failure = 1
}


/// <summary>
/// The available processing libraries options
/// </summary>
public enum ProcessingLibraries
{
    /// <summary>
    /// Process all libraries
    /// </summary>
    All = 0,
    /// <summary>
    /// Only process the libraries specified
    /// </summary>
    Only = 1,
    /// <summary>
    /// Process all libraries except those specified
    /// </summary>
    AllExcept = 2
}

/// <summary>
/// Types of schedules a task can be triggered at
/// </summary>
public enum TaskType
{
    /// <summary>
    /// At a configured schedule
    /// </summary>
    Schedule = 0,

    /// <summary>
    /// When a file is added to the system
    /// </summary>
    FileAdded = 1,

    /// <summary>
    /// When a file starts processing
    /// </summary>
    FileProcessing = 2,

    /// <summary>
    /// When a file has been processed
    /// </summary>
    FileProcessed = 3,

    /// <summary>
    /// When a file was successfully processed
    /// </summary>
    FileProcessSuccess = 4,

    /// <summary>
    /// When a file failed processing
    /// </summary>
    FileProcessFailed = 5,

    /// <summary>
    /// When a update to FileFlows is available
    /// </summary>
    FileFlowsServerUpdateAvailable = 100,
    /// <summary>
    /// When FileFlows is updating
    /// </summary>
    FileFlowsServerUpdating = 101

}

/// <summary>
/// Match
/// </summary>
public enum MatchRange 
{
    /// <summary>
    /// Any 
    /// </summary>
    Any = 0,
    /// <summary>
    /// Greater than value specified
    /// </summary>
    GreaterThan = 1,
    /// <summary>
    /// Less than value specified
    /// </summary>
    LessThan = 2,
    /// <summary>
    /// Between values specified
    /// </summary>
    Between = 3,
    /// <summary>
    /// Not between values specified
    /// </summary>
    NotBetween = 4
}


/// <summary>
/// Processing priority, used to prioritize library files for processing
/// </summary>
public enum ProcessingPriority
{
    /// <summary>
    /// Lowest priority
    /// </summary>
    Lowest = -10,
    /// <summary>
    /// Low priority
    /// </summary>
    Low = -5,
    /// <summary>
    /// Normal priority
    /// </summary>
    Normal = 0,
    /// <summary>
    /// High priority
    /// </summary>
    High = 5,
    /// <summary>
    /// Highest priority
    /// </summary>
    Highest = 10
}

/// <summary>
/// Processing order for a library
/// </summary>
public enum ProcessingOrder
{
    /// <summary>
    /// Default order, as they are found
    /// </summary>
    AsFound = 0,
    /// <summary>
    /// Randomly
    /// </summary>
    Random = 1,
    /// <summary>
    /// Smallest files first
    /// </summary>
    SmallestFirst = 2,
    /// <summary>
    /// Largest files first
    /// </summary>
    LargestFirst = 3,
    /// <summary>
    /// Newest files first
    /// </summary>
    NewestFirst = 4,
    /// <summary>
    /// Oldest files first
    /// </summary>
    OldestFirst = 5
}


/// <summary>
/// Methods for an HTTP request
/// </summary>
public enum HttpMethod
{
    /// <summary>
    /// Get request
    /// </summary>
    Get = 0,
    /// <summary>
    /// Post request
    /// </summary>
    Post = 1,
    /// <summary>
    /// Put request
    /// </summary>
    Put = 2,
    /// <summary>
    /// Delete request
    /// </summary>
    Delete = 3
}

/// <summary>
/// License flags
/// </summary>
[Flags]
public enum LicenseFlags
{
    /// <summary>
    /// Not licensed
    /// </summary>
    NotLicensed = 0,
    /// <summary>
    /// Allowed to use an external database
    /// </summary>
    ExternalDatabase = 1,
    /// <summary>
    /// Allowed to use auto updates
    /// </summary>
    AutoUpdates = 2,
    /// <summary>
    /// Allowed advanced dashboards
    /// </summary>
    Dashboards = 4,
    /// <summary>
    /// Allowed to access revisions
    /// </summary>
    Revisions = 8,
    /// <summary>
    /// Can execute tasks
    /// </summary>
    Tasks = 16,
    /// <summary>
    /// Can use webhooks
    /// </summary>
    Webhooks = 32
}