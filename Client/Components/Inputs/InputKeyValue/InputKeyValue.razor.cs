using FileFlows.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace FileFlows.Client.Components.Inputs;

/// <summary>
/// Input for a KeyValue pair
/// </summary>
public partial class InputKeyValue : Input<List<KeyValuePair<string, string>>>
{
    private string InputText = "";
    private string PreviousInputText = "";
    private string NewKey = string.Empty;
    private string NewValue = string.Empty;
    /// <summary>
    /// Focuses the input
    /// </summary>
    /// <returns>true if the input gained focus, otherwise false</returns>
    public override bool Focus() => FocusUid();

    /// <summary>
    /// The data for the input
    /// </summary>
    private List<KeyValue> Data = new List<KeyValue>();

    private string DuplicateKey = null; // one time we do want null....

    private string lblKey, lblValue;

    /// <summary>
    /// Initializes the component
    /// </summary>
    protected override void OnInitialized()
    {
        lblKey = Translater.Instant(this.LabelOriginal + "Key");
        lblValue = Translater.Instant(this.LabelOriginal + "Value");
        base.OnInitialized();
        if (Value == null)
            Value = new List<KeyValuePair<string, string>>();

        this.Data = Value.Select(x => new KeyValue {  Key = x.Key, Value = x.Value }).ToList();
        if(Field != null)
            this.Field.ValueChanged += FieldOnValueChanged; 
    }

    private void FieldOnValueChanged(object sender, object value)
    {
        if (value == null)
            return;
        if(value is List<KeyValuePair<string,string>> kvps == false)
            return;
        bool differences = false;
        foreach (var kvp in kvps)
        {
            var existing = this.Data.FirstOrDefault(x => x.Key == kvp.Key);
            if (existing == null)
            {
                Data.Add(new() { Key = kvp.Key, Value = kvp.Value });
                differences = true;
            }
            else if (existing.Value != kvp.Value)
            {
                existing.Value = kvp.Value;
                differences = true;
            }
        }

        if (differences)
            this.StateHasChanged();
    }


    /// <summary>
    /// Remove an item from the list
    /// </summary>
    /// <param name="kv">the item to remove</param>
    void Remove(KeyValue kv)
    {
        this.Data.Remove(kv);
        CheckForDuplicates();
    }

    /// <summary>
    /// Add a new item to the list
    /// </summary>
    void Add()
    {
        string key = NewKey;
        string value = NewValue ?? string.Empty;
        if (string.IsNullOrWhiteSpace(key))
            return;
        key = key.Trim();

        this.Data.Add(new KeyValue {  Key = key, Value = value });

        CheckForDuplicates();

        NewKey = string.Empty;
        NewValue = string.Empty;
        FocusUid();
        UpdateBindValue();
    }

    /// <summary>
    /// When the input loses focus
    /// </summary>
    void OnBlur()
    {
        // CheckForDuplicates();
        UpdateBindValue();
    }

    bool UpdateBindValue()
    {
        this.Data ??= new();

        if (CheckForDuplicates() == false)
            return false;


        if (this.Data.Any() == false && Validators?.Any(x => x.Type == "Required") == true)
        {
            this.ErrorMessage = Translater.Instant("Validators.Required");
            return false;
        }


        this.Value = this.Data.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList();
        return true;
    }

    /// <summary>
    /// Validates the input
    /// </summary>
    /// <returns>whether or not the input is valid</returns>
    public override async Task<bool> Validate()
    {
        if (UpdateBindValue() == false)
            return false;

        return await base.Validate();
    }

    /// <summary>
    /// Check the input for duplicates
    /// </summary>
    /// <returns>true if there are duplicates, otherwise false</returns>
    private bool CheckForDuplicates()
    {
        DuplicateKey = this.Data?.GroupBy(x => x.Key, x => x)?.FirstOrDefault(x => x.Count() > 1)?.Select(x => x.Key)?.FirstOrDefault();
        if (DuplicateKey != null)
        {
            Logger.Instance.WLog("Duplicates found, " + DuplicateKey);
            ErrorMessage = Translater.Instant("ErrorMessages.DuplicatesFound");
            this.StateHasChanged();
            return false;
        }
        ErrorMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// A key value
    /// </summary>
    class KeyValue
    {
        /// <summary>
        /// Gets or sets the key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }   
    }
}