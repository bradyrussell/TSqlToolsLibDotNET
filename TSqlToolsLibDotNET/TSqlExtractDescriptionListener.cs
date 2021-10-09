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
            descriptions[string.Join(".", currentSchema, currentTable, currentColumn)] = currentDescription;
            resetCurrentValues();
        }

        override public void EnterExecute_statement(TSqlParser.Execute_statementContext ctx)
        {
            foreach (TSqlParser.Execute_statement_arg_namedContext s in ctx.execute_body().execute_statement_arg().execute_statement_arg_named())
            {
                string value = readTSqlNstring(s.execute_parameter().GetText());
                switch (s.LOCAL_ID().GetText())
                {
                    case "@name":
                        {
                            if (!value.Equals("MS_Description"))
                            {
                                resetCurrentValues();
                                return;
                            }
                            break;
                        }
                    case "@value":
                        {
                            currentDescription = value;
                            break;
                        }
                    case "@level0name":
                        {
                            currentSchema = value;
                            break;
                        }
                    case "@level1name":
                        {
                            currentTable = value;
                            break;
                        }
                    case "@level2name":
                        {
                            currentColumn = value;
                            break;
                        }
                }
            }
            saveDescription();
        }

        public string readTSqlNstring(string nstring)
        {
            return nstring.StartsWith("N'") ? nstring.Substring(2, nstring.Length - 1).Replace("''", "'") : nstring;
        }

        public Dictionary<string, string> getDescriptions()
        {
            return descriptions;
        }
    }
}
