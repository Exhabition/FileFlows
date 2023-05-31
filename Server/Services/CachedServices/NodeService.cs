﻿using System.ComponentModel;
using System.Runtime.InteropServices;
using Esprima.Ast;
using FileFlows.Server.Helpers;

namespace FileFlows.Server.Services;

using FileFlows.Server.Controllers;
using FileFlows.ServerShared.Services;
using FileFlows.Shared.Models;
using System;
using System.Threading.Tasks;

/// <summary>
/// An Service for communicating with the server for all Processing Node related actions
/// </summary>
public class NodeService : CachedService<ProcessingNode>, INodeService
{
    public override bool IncrementsConfiguration => false;

    static NodeService()
    {
        if(Globals.IsUnitTesting == false)
            new NodeService().Refresh();
    }


/// <summary>
    /// A loader to load an instance of the Node service
    /// </summary>
    public static Func<INodeService> Loader { get; set; }

    /// <summary>
    /// Loads an instance of the node service
    /// </summary>
    /// <returns>an instance of the node service</returns>
    public static INodeService Load()
    {
        if (Loader == null)
            return new NodeService();
        return Loader.Invoke();
    }
    /// <summary>
    /// Clears all workers on the node.
    /// This is called when a node first starts up, if a node crashed when workers were running this will reset them
    /// </summary>
    /// <param name="nodeUid">The UID of the node</param>
    /// <returns>a completed task</returns>
    public Task ClearWorkersAsync(Guid nodeUid) => new WorkerController(null).Clear(nodeUid);

    /// <summary>
    /// Gets an instance of the internal processing node
    /// </summary>
    /// <returns>an instance of the internal processing node</returns>
    public async Task<ProcessingNode> GetServerNodeAsync()
    {
        var node = Data.FirstOrDefault(x => x.Uid == Globals.InternalNodeUid);
        if (node != null)
            return node;
        
        Logger.Instance.ILog("Adding Internal Processing Node");
        bool windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);                
        node = await DbHelper.Update(new ProcessingNode
        {
        
            Uid = Globals.InternalNodeUid,
            Name = Globals.InternalNodeName,
            Address = Globals.InternalNodeName,
            Schedule = new string('1', 672),
            Enabled = true,
            FlowRunners = 1,
            Version = Globals.Version.ToString(),
            AllLibraries = ProcessingLibraries.All,
#if (DEBUG)
            TempPath = windows ? @"d:\videos\temp" : Path.Combine(DirectoryHelper.BaseDirectory, "Temp"),
#else
            TempPath = DirectoryHelper.IsDocker ? "/temp" : Path.Combine(DirectoryHelper.BaseDirectory, "Temp"),
#endif
        });
        node.SignalrUrl = "flow";
        return node;
    } 

    /// <summary>
    /// Gets a tool path by name
    /// </summary>
    /// <param name="name">The name of the tool</param>
    /// <returns>a tool path</returns>
    public Task<string> GetVariableAsync(string name)
    {
        var result = new VariableController().GetByName(name);
        return Task.FromResult(result?.Value ?? string.Empty);
    }

    /// <summary>
    /// Gets a processing node by its physical address
    /// </summary>
    /// <param name="address">The address (hostname or IP address) of the node</param>
    /// <returns>An instance of the processing node</returns>
    public async Task<ProcessingNode> GetByAddressAsync(string address)
    {
        if (address == "INTERNAL_NODE")
            return await GetServerNodeAsync();
        address = address.Trim().ToLowerInvariant();
        var node = Data.FirstOrDefault(x => x.Address.ToLowerInvariant() == address);
        return node!;
    }

    /// <summary>
    /// Updates the version of a Node
    /// </summary>
    /// <param name="uid">the unique identifier for the node</param>
    /// <param name="version">the new version</param>
    public async Task UpdateVersion(Guid uid, string version)
    {
        var item = GetByUid(uid);
        if (item == null)
            return;
        if(Version.TryParse(item.Version ?? "0.0.0.0", out Version? vOld) == false)
            vOld = new Version();
        if(Version.TryParse(version ?? "0.0.0.0", out Version? vNew) == false)
            vNew = new Version();
        if (vOld == vNew)
            return; // nothing to do
        item.Version = version;
        await DbHelper.UpdateJsonProperty(uid, nameof(ProcessingNode.Version), version);
    }

    /// <summary>
    /// Changes the temp path of a node
    /// </summary>
    /// <param name="address">the nodes address</param>
    /// <param name="path">the new temp path</param>
    /// <returns>the result</returns>
    public async Task ChangeTempPath(string address, string path)
    {
        if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(path))
            return;
        var node = await GetByAddressAsync(address);
        if (node == null)
            return;
        if (node.TempPath == path)
            return;
        node.TempPath = path;
        await DbHelper.UpdateJsonProperty(node.Uid, nameof(node.TempPath), path);
    }

    /// <summary>
    /// Set state of a processing node
    /// </summary>
    /// <param name="uid">The UID of the processing node</param>
    /// <param name="enable">Whether or not this node is enabled and will process files</param>
    /// <returns>an awaited task</returns>
    public async Task SetState(Guid uid, bool enable)
    {
        var node = await GetByUidAsync(uid);
        if (node == null)
            return;
        await DbHelper.UpdateJsonProperty(node.Uid, nameof(node.Enabled), enable);
    }
}