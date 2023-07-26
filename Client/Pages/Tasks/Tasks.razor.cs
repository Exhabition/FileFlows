using FileFlows.Client.Components;
using FileFlows.Client.Components.Inputs;
using FileFlows.Plugin;
using Humanizer;

namespace FileFlows.Client.Pages;

public partial class Tasks: ListPage<Guid, FileFlowsTask>
{
    public override string ApiUrl => "/api/task";

    private string lblLastRun, lblNever, lblTrigger;
    private string lblRunAt, lblSuccess, lblReturnCode;

    private enum TimeSchedule
    {
        Hourly,
        Every3Hours,
        Every6Hours,
        Every12Hours,
        Daily,
        Custom
    }

    private static readonly string SCHEDULE_HOURLY = string.Concat(Enumerable.Repeat("1000", 24 * 7));
    private static readonly string SCHEDULE_3_HOURLY = string.Concat(Enumerable.Repeat("1" + new string('0', 11), 8 * 7));
    private static readonly string SCHEDULE_6_HOURLY = string.Concat(Enumerable.Repeat("1" + new string('0', 23), 4 * 7));
    private static readonly string SCHEDULE_12_HOURLY = string.Concat(Enumerable.Repeat("1" + new string('0', 47), 2 * 7));
    private static readonly string SCHEDULE_DAILY = string.Concat(Enumerable.Repeat("1" + new string('0', 95), 7));


    /// <summary>
    /// Gets if they are licensed for this page
    /// </summary>
    /// <returns>if they are licensed for this page</returns>
    protected override bool Licensed()
        => App.Instance.FileFlowsSystem.LicenseTasks; 
    
    protected override void OnInitialized()
    {
        lblNever = Translater.Instant("Labels.Never");
        lblLastRun = Translater.Instant("Labels.LastRun");
        lblTrigger = Translater.Instant("Labels.Trigger");
        lblRunAt = Translater.Instant("Labels.RunAt");
        lblSuccess = Translater.Instant("Labels.Success");
        lblReturnCode = Translater.Instant("Labels.ReturnCode");
        base.OnInitialized();
    }

    private string GetSchedule(FileFlowsTask task)
    {
        if (task.Type != TaskType.Schedule)
            return string.Empty;
        if (task.Schedule == SCHEDULE_HOURLY) return "Hourly";
        if (task.Schedule == SCHEDULE_3_HOURLY) return "Every 3 Hours";
        if (task.Schedule == SCHEDULE_6_HOURLY) return "Every 6 Hours";
        if (task.Schedule == SCHEDULE_12_HOURLY) return "Every 12 Hours";
        if (task.Schedule == SCHEDULE_DAILY) return "Daily";
        return "Custom Schedule";
    }
    
    private async Task Add()
    {
        await Edit(new FileFlowsTask()
        {
            Schedule = SCHEDULE_DAILY
        });
    }
    
    public override async Task<bool> Edit(FileFlowsTask item)
    {
        List<ElementField> fields = new List<ElementField>();

        var scriptResponse = await HttpHelper.Get<List<Script>>("/api/script/list/system");
        if (scriptResponse.Success == false)
        {
            Toast.ShowError(scriptResponse.Body);
            return false;
        }

        if (scriptResponse.Data?.Any() != true)
        {
            Toast.ShowError("Pages.Tasks.Messages.NoScripts");
            return false;
        }

        var scriptOptions = scriptResponse.Data.Select(x => new ListOption()
        {
            Label = x.Name,
            Value = x.Uid
        }).ToList();

        fields.Add(new ElementField
        {
            InputType = FormInputType.Text,
            Name = nameof(item.Name),
            Validators = new List<FileFlows.Shared.Validators.Validator> {
                new FileFlows.Shared.Validators.Required()
            }
        });
        fields.Add(new ElementField
        {
            InputType = FormInputType.Select,
            Name = nameof(item.Script),
            Parameters = new ()
            {
                { nameof(InputSelect.Options), scriptOptions }
            }
        });
        var efTaskType = new ElementField
        {
            InputType = FormInputType.Select,
            Name = nameof(item.Type),
            Parameters = new()
            {
                { nameof(InputSelect.AllowClear), false },
                {
                    nameof(InputSelect.Options),
                    Enum.GetValues<TaskType>().Select(x => new ListOption()
                        { Label = x.Humanize(LetterCasing.Title).Replace("File Flows", "FileFlows"), Value = x }).ToList()
                }
            }
        };
        fields.Add(efTaskType);

        TimeSchedule timeSchedule = TimeSchedule.Hourly;
        if (item.Type == TaskType.Schedule)
        {
            if (item.Schedule == SCHEDULE_DAILY)
                timeSchedule = TimeSchedule.Daily;
            else if (item.Schedule == SCHEDULE_12_HOURLY)
                timeSchedule = TimeSchedule.Every12Hours;
            else if (item.Schedule == SCHEDULE_6_HOURLY)
                timeSchedule = TimeSchedule.Every6Hours;
            else if (item.Schedule == SCHEDULE_3_HOURLY)
                timeSchedule = TimeSchedule.Every3Hours;
            else if (item.Schedule == SCHEDULE_HOURLY)
                timeSchedule = TimeSchedule.Hourly;
            else if(item.Schedule != new string('0', 672))
                timeSchedule = TimeSchedule.Custom;
        }

        var efSchedule = new ElementField
        {
            InputType = FormInputType.Select,
            Name = "TimeSchedule",
            Parameters = new()
            {
                { nameof(InputSelect.AllowClear), false },
                {
                    nameof(InputSelect.Options),
                    Enum.GetValues<TimeSchedule>().Select(x => new ListOption()
                        { Label = x.Humanize(LetterCasing.Title), Value = x }).ToList()
                }
            },
            Conditions = new List<Condition>
            {
                new(efTaskType, item.Type, value: TaskType.Schedule)
            }
        };
        fields.Add(efSchedule);

        string customSchedule = SCHEDULE_HOURLY;
        if (item.Type == TaskType.Schedule)
        {
            if (item.Schedule == SCHEDULE_DAILY)
                timeSchedule = TimeSchedule.Daily;
            else if (item.Schedule == SCHEDULE_3_HOURLY)
                timeSchedule = TimeSchedule.Every3Hours;
            else if (item.Schedule == SCHEDULE_6_HOURLY)
                timeSchedule = TimeSchedule.Every6Hours;
            else if (item.Schedule == SCHEDULE_12_HOURLY)
                timeSchedule = TimeSchedule.Every12Hours;
            else if (item.Schedule == SCHEDULE_DAILY)
                timeSchedule = TimeSchedule.Daily;
            else if (item.Schedule == SCHEDULE_HOURLY)
                timeSchedule = TimeSchedule.Hourly;
            else
                timeSchedule = TimeSchedule.Custom;
            customSchedule = item.Schedule?.EmptyAsNull() ?? SCHEDULE_HOURLY;
        }
        
        fields.Add(new ElementField
        {
            InputType = FormInputType.Schedule,
            Name = "CustomSchedule",
            Parameters = new ()
            {
                { nameof(InputSchedule.HideLabel), true }
            },
            Conditions = new List<Condition>
            {
                new (efTaskType, item.Type, value: TaskType.Schedule),
                new (efSchedule, timeSchedule, value: TimeSchedule.Custom)
            }
        });
        await Editor.Open(new()
        {
            TypeName = "Pages.Task", Title = "Pages.Task.Title", Fields = fields, Model = new
            {
                item.Uid,
                item.Name,
                item.Script,
                item.Type,
                CustomSchedule = customSchedule,
                TimeSchedule = timeSchedule
            },
            SaveCallback = Save
        });
        
        return false;
    }
    
    
    async Task<bool> Save(ExpandoObject model)
    {
        Blocker.Show();
        this.StateHasChanged();
        var task = new FileFlowsTask();
        var dict = model as IDictionary<string, object>;
        task.Name = dict["Name"].ToString() ?? string.Empty;
        task.Script = dict["Script"].ToString() ?? string.Empty;
        task.Uid = (Guid)dict["Uid"];
        task.Type = (TaskType)dict["Type"];
        if (task.Type == TaskType.Schedule)
        {
            var timeSchedule = (TimeSchedule)dict["TimeSchedule"];
            switch (timeSchedule)
            {
                case TimeSchedule.Daily: task.Schedule = SCHEDULE_DAILY;
                    break;
                case TimeSchedule.Hourly: task.Schedule = SCHEDULE_HOURLY;
                    break;
                case TimeSchedule.Every3Hours: task.Schedule = SCHEDULE_3_HOURLY;
                    break;
                case TimeSchedule.Every6Hours: task.Schedule = SCHEDULE_6_HOURLY;
                    break;
                case TimeSchedule.Every12Hours: task.Schedule = SCHEDULE_12_HOURLY;
                    break;
                default:
                    task.Schedule = (string)dict["CustomSchedule"];
                    break;
            }
        }

        try
        {
            var saveResult = await HttpHelper.Post<FileFlowsTask>($"{ApiUrl}", task);
            if (saveResult.Success == false)
            {
                Toast.ShowError( saveResult.Body?.EmptyAsNull() ?? Translater.Instant("ErrorMessages.SaveFailed"));
                return false;
            }

            int index = this.Data.FindIndex(x => x.Uid == saveResult.Data.Uid);
            if (index < 0)
                this.Data.Add(saveResult.Data);
            else
                this.Data[index] = saveResult.Data;
            await this.Load(saveResult.Data.Uid);

            return true;
        }
        finally
        {
            Blocker.Hide();
            this.StateHasChanged();
        }
    }
    
    

    async Task Run()
    {
        var item = Table.GetSelected()?.FirstOrDefault();
        if (item == null)
            return;
        try
        {
            var result = await HttpHelper.Post<FileFlowsTaskRun>($"/api/task/run/{item.Uid}");
            if (result.Success && result.Data.Success)
            {
                Toast.ShowSuccess("Script executed");
            }
            else
            {
                Toast.ShowError(result?.Data?.Log?.EmptyAsNull() ?? result.Body?.EmptyAsNull() ?? "Failed to run task");
            }

            await Refresh();
        }
        finally
        {
            this.Blocker.Hide();
        }
    }


    async Task RunHistory()
    {
        var item = Table.GetSelected()?.FirstOrDefault();
        if (item == null)
            return;

        var blockerHidden = false;
        Blocker.Show();
        try
        {
            var taskResponse = await HttpHelper.Get<FileFlowsTask>("/api/task/" + item.Uid);
            if (taskResponse.Success == false)
            {
                Toast.ShowError(taskResponse.Body);
                return;
            }

            if (taskResponse.Data?.RunHistory?.Any() != true)
            {
                Toast.ShowInfo("Pages.Tasks.Messages.NoHistory");
                return;
            }

            var task = taskResponse.Data;
            Blocker.Hide();
            blockerHidden = true;
            _ = TaskHistory(task);
        }
        finally
        {
            if (blockerHidden == false)
                Blocker.Hide();
        }
    }

    async Task TaskHistory(FileFlowsTask task)
    {
        List<ElementField> fields = new List<ElementField>();
        fields.Add(new ()
        {
            InputType = FormInputType.Table,
            Name = nameof(task.RunHistory),
            Parameters = new ()
            {
                { nameof(InputTable.TableType), typeof(FileFlowsTaskRun) },
                { nameof(InputTable.Columns) , new List<InputTableColumn>
                {
                    new () { Name = lblRunAt, Property = nameof(FileFlowsTaskRun.RunAt) },
                    new () { Name = lblSuccess, Property = nameof(FileFlowsTaskRun.Success) },
                    new () { Name = lblReturnCode, Property = nameof(FileFlowsTaskRun.ReturnValue) }
                }},
                { nameof(InputTable.SelectAction), new Action<object>(item =>
                {
                    if (item is FileFlowsTaskRun run == false)
                        return; // should never happen
                    _ = TaskRun(run);
                })}
            }
        });
        
        await Editor.Open(new()
        {
            TypeName = "Pages.FileFlowsTaskHistory", Title = "Pages.FileFlowsTaskHistory.Title",
            ReadOnly = true,
            Fields = fields,
            Model = new {
                RunHistory = task.RunHistory.OrderByDescending(x => x.RunAt).ToList()
            }
        });
        
    }

    async Task TaskRun(FileFlowsTaskRun fileFlowsTaskRun)
    {
        List<ElementField> fields = new List<ElementField>();

        fields.Add(new ElementField
        {
            InputType = FormInputType.TextLabel,
            Name = nameof(fileFlowsTaskRun.RunAt)
        });
        fields.Add(new ElementField
        {
            InputType = FormInputType.TextLabel,
            Name = nameof(fileFlowsTaskRun.Success)
        });
        fields.Add(new ElementField
        {
            InputType = FormInputType.TextLabel,
            Name = nameof(fileFlowsTaskRun.ReturnValue)
        });
        fields.Add(new ElementField
        {
            InputType = FormInputType.LogView,
            Name = nameof(fileFlowsTaskRun.Log)
        });

        await Editor.Open(new()
        {
            TypeName = "Pages.FileFlowsTaskRun", Title = "Pages.FileFlowsTaskRun.Title",
            Model = fileFlowsTaskRun,
            ReadOnly = true,
            Fields = fields
        });
    }
}