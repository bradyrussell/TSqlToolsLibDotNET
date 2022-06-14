using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TSqlToolsLib
{
    public class TSqlAnnotateCodeValuesListener : TSqlParserBaseListener
    {
        private static String annotationCommentBeginSymbol = "@";
        private static String annotationCommentEndSymbol = "@";

        private CommonTokenStream tokens;
        private TokenStreamRewriter rewriter;
        private string database;
        private string server;

        public TSqlAnnotateCodeValuesListener(CommonTokenStream tokens, string server, string database)
        {
            this.tokens = tokens;
            this.server = server;
            this.database = database;
            this.rewriter = new TokenStreamRewriter(tokens);
        }

        private string GetCodeValueFromDatabase(int codekey)
        {
            using (var sql = new SqlConnection(GetConnectionString(server)))
            {
                string query = $@"select top 1 codeval from {database}.dbo.codevalues where codekey = {codekey}";

                sql.Open();
                using (SqlCommand cmd = new SqlCommand(query, sql))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader[0] == null || reader[0] == DBNull.Value ? null : reader[0].ToString();
                        }
                    }
                }
            }

            return null;
        }

        public static string GetConnectionString(string dataSource, int timeoutSeconds = 5)
        {
            return $"Data Source={dataSource};Integrated Security=True;Connection Timeout={timeoutSeconds};";
        }

        public override void EnterConstant(TSqlParser.ConstantContext context)
        {
            if (int.TryParse(context.GetText(), out int codekey))
            {
                string cv = GetCodeValueFromDatabase(codekey);
                if (!string.IsNullOrWhiteSpace(cv))
                {
                    rewriter.InsertAfter(context.Stop.TokenIndex, $" /*{annotationCommentBeginSymbol} {cv} {annotationCommentEndSymbol}*/");
                }
            }
        }

        public String getResultTSql()
        {
            return rewriter.GetText();
        }
    }

}
