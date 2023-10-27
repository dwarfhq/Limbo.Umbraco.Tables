﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using Limbo.Umbraco.Tables.Parsers;
using Limbo.Umbraco.Tables.PropertyEditors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Newtonsoft.Extensions;

namespace Limbo.Umbraco.Tables.Models {

    /// <summary>
    /// Class representing the value of a <see cref="TablesDataEditor"/>.
    /// </summary>
    public class TablesDataModel : TablesDataObject {

        #region Properties

        /// <summary>
        /// Gets whether the first row of the table should be used as a header.
        /// </summary>
        [JsonProperty("useFirstRowAsHeader")]
        [JsonPropertyName("useFirstRowAsHeader")]
        public bool UseFirstRowAsHeader { get; }

        /// <summary>
        /// Gets whether the first column of the table should be used as a header.
        /// </summary>
        [JsonProperty("useFirstColumnAsHeader")]
        [JsonPropertyName("useFirstColumnAsHeader")]
        public bool UseFirstColumnAsHeader { get; }

        /// <summary>
        /// Gets whether the last row of the table should be used as a footer.
        /// </summary>
        [JsonProperty("useLastRowAsFooter")]
        [JsonPropertyName("useLastRowAsFooter")]
        public bool UseLastRowAsFooter { get; }

        /// <summary>
        /// Gets a list of the rows in the structued data table.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IReadOnlyList<TablesDataRow> Rows { get; }

        /// <summary>
        /// Gets a list of the columns in the structued data table.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IReadOnlyList<TablesDataColumn> Columns { get; }

        /// <summary>
        /// Gets a list of the cells in the structued data table.
        /// </summary>
        [JsonProperty("cells")]
        [JsonPropertyName("cells")]
        public IReadOnlyList<IReadOnlyList<TablesDataCell>> Cells { get; }

        #endregion

        #region Constructors

        private TablesDataModel(JObject json, TablesDataConfiguration config, TablesHtmlParser htmlParser, bool preview) : base(json) {

            UseFirstRowAsHeader = json.GetBoolean("useFirstRowAsHeader") && config.AllowUseFirstRowAsHeader;
            UseFirstColumnAsHeader = json.GetBoolean("useFirstColumnAsHeader") && config.AllowUseFirstColumnAsHeader;
            UseLastRowAsFooter = json.GetBoolean("useLastRowAsFooter") && config.AllowUseLastRowAsFooter;

            JArray rows = json.GetArrayOrNew("rows");

            Rows = json.GetArrayOrNew("rows")
                .ForEach((i, x) => new TablesDataRow(i, x, rows.Count, this))
                .ToList();

            Columns = json.GetArrayOrNew("columns")
                .ForEach((i, x) => new TablesDataColumn(i, x, this))
                .ToList();

            Cells = json
                .GetArrayOrNew("cells")
                .ForEach((i, x) => ParseCellRow(i, x, htmlParser, preview))
                .ToList();

        }

        #endregion

        #region Member methods

        private List<TablesDataCell> ParseCellRow(int index, JArray array, TablesHtmlParser htmlParser, bool preview) {

            TablesDataRow row = Rows[index];

            List<TablesDataCell> temp = new();

            for (int c = 0; c < array.Count; c++) {

                int columnIndex = c;
                TablesDataColumn column = Columns[columnIndex];

                temp.Add(array.GetObject(c, x => new TablesDataCell(x, index, row, columnIndex, column, htmlParser, preview))!);

            }

            return temp;

        }

        #endregion

        #region Static methods

        /// <summary>
        /// Returns a new instance of <see cref="TablesDataModel"/> parsed from the specified <paramref name="json"/> object, or <c>null</c> if <paramref name="json"/> is null.
        /// </summary>
        /// <param name="json">The JSON object.</param>
        /// <param name="config">The table configuration.</param>
        /// <param name="htmlParser">An instance of <see cref="TablesHtmlParser"/> to be used for parsing HTML values.</param>
        /// <param name="preview">Whether the model is part of a page being viewed in preview mode.</param>
        /// <returns>An instance of <see cref="TablesDataModel"/>, or <c>null</c> if <paramref name="json"/> is null.</returns>
        [return: NotNullIfNotNull("json")]
        public static TablesDataModel? Parse(JObject? json, TablesDataConfiguration config, TablesHtmlParser htmlParser, bool preview) {
            return json == null ? null : new TablesDataModel(json, config, htmlParser, preview);
        }

        #endregion

    }

}