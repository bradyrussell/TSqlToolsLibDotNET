using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TSqlToolsLib
{
    class TSqlDescriptionFromCommentsListener : TSqlParserBaseListener
    {
        private static string descriptionCommentBeginSymbol = "$";
        private static string descriptionCommentEndSymbol = "$";

        private CommonTokenStream tokens;
        private List<string> columnDescriptions = new List<string>();

        private string currentTable = null;
        private string currentSchema = null;

        public TSqlDescriptionFromCommentsListener(CommonTokenStream tokens)
        {
            this.tokens = tokens;
        }

        public override void EnterCreate_table(TSqlParser.Create_tableContext ctx)
        {
            if (ctx.table_name() == null) return;
            if (ctx.table_name().schema != null)
            {
                currentSchema = RemoveBrackets(ctx.table_name().schema.GetText());
            }
            else
            {
                currentSchema = null;
            }

            if (ctx.table_name().table != null)
            {
                currentTable = RemoveBrackets(ctx.table_name().table.GetText());
            }
            else
            {
                currentTable = null;
            }
            Debug.WriteLine("Current Schema: "+ currentSchema);
            Debug.WriteLine("Current Table: "+ currentTable);
        }


        public override void EnterColumn_definition(TSqlParser.Column_definitionContext ctx)
        {
            string columnName = RemoveBrackets(ctx.id_()[0].GetText());
            Debug.WriteLine(columnName);

            int offset = 0;
            if (tokens.Get(ctx.Stop.TokenIndex + 1).Type == TSqlParser.COMMA)
            {
                offset = 1;
            }

            IList<IToken> comments = tokens.GetHiddenTokensToRight(ctx.Stop.TokenIndex + offset, 1);

            if (comments == null) return;

            if (comments.Count > 1)
            {
                return;
            }

            if (comments.Count == 1)
            {
                string commentText = comments[0].Text;
                Debug.WriteLine("Found comment: "+ commentText);

                if (commentText.Contains(descriptionCommentBeginSymbol))
                {
                    Debug.WriteLine("Comment contains description!");
                    int index = commentText.IndexOf(descriptionCommentBeginSymbol);
                    string descriptionFromComment = commentText.Substring(index + descriptionCommentBeginSymbol.Length);
                    if (descriptionFromComment.Contains(descriptionCommentEndSymbol))
                    {
                        int index2 = descriptionFromComment.IndexOf(descriptionCommentEndSymbol);
                        descriptionFromComment = descriptionFromComment.Substring(0, index2);
                    }
                    Debug.WriteLine("Description for column " + columnName + ": " + descriptionFromComment);

                    columnDescriptions.Add(MakeDescription(currentSchema, currentTable, columnName, descriptionFromComment.Replace("'", "''")));
                }
                else
                {
                    Debug.WriteLine("Comment is regular comment!");
                }
            }
        }

        public List<string> GetColumnDescriptions()
        {
            return columnDescriptions;
        }

        private string RemoveBrackets(string input)
        {
            return input.Replace("[", "").Replace("]", "");
        }

        public string MakeDescription(string schema, string table, string column, string description)
        {
            return $"EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{description}' , @level0type=N'SCHEMA',@level0name=N'{schema ?? ""}', @level1type=N'TABLE',@level1name=N'{table ?? ""}', @level2type=N'COLUMN',@level2name=N'{column}'\nGO";
        }
    }
}
