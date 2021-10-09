using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;

namespace TSqlToolsLib
{
    public class TSqlExtractDescriptionListener : TSqlParserBaseListener
    {
        private CommonTokenStream tokens;
        private TokenStreamRewriter rewriter;

        public TSqlExtractDescriptionListener(CommonTokenStream tokens)
        {
            this.tokens = tokens;
            this.rewriter = new TokenStreamRewriter(tokens);
        }

        private Dictionary<string, string> descriptions = new Dictionary<string, string>();

        private string currentColumn = "";
        private string currentTable = "";
        private string currentSchema = "";
        private string currentDescription = "";

        private void resetCurrentValues()
        {
            currentColumn = "";
            currentDescription = "";
            currentSchema = "";
            currentTable = "";
        }

        private void saveDescription()
        {
            descriptions.Add(string.Join(".", currentSchema, currentTable, currentColumn), currentDescription);
            resetCurrentValues();
        }

        override public void EnterExecute_statement(TSqlParser.Execute_statementContext ctx)
        {
            var y = ctx.execute_body().execute_statement_arg().execute_statement_arg_named();
            foreach (TSqlParser.Execute_statement_arg_namedContext s in ctx.execute_body().execute_statement_arg().execute_statement_arg_named())
            {
                string value = readTSqlNstring(s.execute_parameter().GetText());
                var id = s.LOCAL_ID().GetText();

                if (id == "@name")
                {
                    if (value != "MS_Description")
                    {
                        resetCurrentValues();
                        return;
                    }
                }
                else if (id == "@value")
                {
                    currentDescription = value;
                }
                else if (id == "@level0name")
                {
                    currentSchema = value;
                }
                else if (id == "@level1name")
                {
                    currentTable = value;
                }
                else if (id == "@level2name")
                {
                    currentColumn = value;
                }
            }
            saveDescription();
        }

        public string readTSqlNstring(string nstring)
        {
            return nstring.StartsWith("N'") ? nstring.Substring(2, nstring.Length - 3).Replace("''", "'") : nstring;
        }

        public Dictionary<string, string> getDescriptions()
        {
            return descriptions;
        }
    }
}
