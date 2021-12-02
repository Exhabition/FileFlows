﻿namespace FileFlows.Client.Components.Common
{
    using FileFlows.Shared;
    using Microsoft.AspNetCore.Components;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public partial class FlowTableButton<TItem> : ComponentBase, IDisposable
    {
        [CascadingParameter] FlowTable<TItem> Table { get; set; }

        private string _Label = string.Empty;
        [Parameter]
        public string Label
        {
            get => _Label;
            set
            {
                this._Label = Translater.TranslateIfNeeded(value ?? string.Empty);
            }
        }

        [Parameter]
        public string Icon { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

        private bool _Enabled = true;
        public bool Enabled
        {
            get
            {
                if (Disabled) return false;
                return _Enabled;
            }
        }

        [Parameter]
        public EventCallback Clicked { get; set; }

        [Parameter]
        public bool SelectedOne { get; set; }
        [Parameter]
        public bool SelectedOneOrMore { get; set; }

        private async Task OnClick()
        {
            await this.Clicked.InvokeAsync();
        }
        protected override void OnInitialized()
        {
            this.Table.AddButton(this);
            this.Table.SelectionChanged += Table_SelectionChanged;

            Table_SelectionChanged(null);
        }


        private void Table_SelectionChanged(List<TItem> items)
        {
            bool current = this.Enabled;
            var count = items?.Count ?? 0;
            if (this.SelectedOne)
                this._Enabled = count == 1;
            else if (this.SelectedOneOrMore)
                this._Enabled = count > 0;
            else
                this._Enabled = true;
            if (current != this.Enabled)
                this.StateHasChanged();
        }

        public void Dispose()
        {
            if (this.Table != null)
                this.Table.SelectionChanged -= Table_SelectionChanged;
        }

    }
}
