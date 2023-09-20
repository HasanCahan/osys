using Microsoft.Practices.EnterpriseLibrary.Data;
using OlcuYonetimSistemi.Models.Edw;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Linq.Expressions;
namespace OlcuYonetimSistemi.Controllers.EDW
{
    public static class DataImport
    {
        public static IEnumerable<TSource> Duplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            var moreThan1 = source.DuplicatesGrouping(selector);
            return moreThan1.SelectMany(i => i);
        }
        public static IEnumerable<IGrouping<TKey, TSource>> DuplicatesGrouping<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            var grouped = source.GroupBy(selector);
            var moreThan1 = grouped.Where(i => i.IsMultiple());
            return moreThan1;
        }
        static void RenameTransformer(IEnumerable<EdwTransformer> trs)
        {
            int i = 1;
        }

        public static IEnumerable<TSource> Duplicates<TSource, TKey>(this IEnumerable<TSource> source)
        {
            return source.Duplicates(i => i);
        }

        public static bool IsMultiple<T>(this IEnumerable<T> source)
        {
            var enumerator = source.GetEnumerator();
            return enumerator.MoveNext() && enumerator.MoveNext();
        }
        static readonly Dictionary<string, string> transformerMapper = new Dictionary<string, string>();
        static readonly Dictionary<string, string> transformerCenterMapper = new Dictionary<string, string>();
        static readonly Dictionary<string, string> statusMapper = new Dictionary<string, string>();
        static DataImport()
        {
            AddDosyaProperty<EdwTransformer>(s => s.PmumNumber, "INAVITAS_TRAFO_ID", transformerMapper);
            AddDosyaProperty<EdwTransformer>(s => s.Comment, "COMMENT1", transformerMapper);
            AddDosyaProperty<EdwStatusHistory>(s => s.Comment, "COMMENT1", statusMapper);
            AddDosyaProperty<EdwStatusHistory>(s => s.EdwConsumptionTypeId, "EDWCONSTYPEID", statusMapper);
            AddDosyaProperty<EdwStatusHistory>(s => s.FiderConsumptionTypeId, "FIDERCONSTYPEID", statusMapper);
            AddDosyaProperty<EdwStatusHistory>(s => s.BaraConsumptionTypeId, "BARACONSTYPEID", statusMapper);
        }
        //static void Generate<T>(Dictionary<string, string> map)
        //{
        //    var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        //    foreach(var itm in props)
        //    {
        //        if (map.ContainsKey(itm.Name))
        //            continue;

        //    }
        //}
        public static DataSet List(string tableName)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            var baseSQL = "select * from " + tableName;
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(baseSQL))
            {
                retval = db.ExecuteDataSet(cmd);
            }
            return retval;
        }
        static MemberExpression Member(Expression e)
        {
            if (e is UnaryExpression)
                return (e as UnaryExpression).Operand as MemberExpression;
            return e as MemberExpression;
        }
        static void AddProperty(LambdaExpression exp, string value, Dictionary<string, string> mapper)
        {
            if (exp == null)
                return;
            MemberExpression member = Member(exp.Body);
            var memberInfo = member.Member;
            if (memberInfo == null)
                return;
            var name = memberInfo.Name;
            if (mapper.ContainsKey(name))
                mapper[name] = value;
            else
                mapper.Add(name, value);
        }
        static void AddDosyaProperty<T>(Expression<Func<T, object>> exp, string value, Dictionary<string, string> mapper)
        {
            AddProperty(exp, value, mapper);
        }
        public static void Import()
        {
            Database db = DatabaseFactory.CreateDatabase();
            //var tms = List("TM_OLD").ToList<EdwTransformerCenter>(transformerCenterMapper);
            //var trs = List("TR_OLD").ToList<EdwTransformer>(transformerMapper).GroupBy(s => new { s.TransformerCenterName, s.Name, s.ReceivedEnergyId, s.DeliveredEnergyId, s.PmumNumber }).Select(s => s.FirstOrDefault()).ToList();
            var shs = List("SH_OLD").ToList<EdwStatusHistory>(statusMapper);
            shs = shs.GroupBy(s => new { s.TransformerCenterName, s.TransformerName, s.EdwConsumptionTypeId, s.BaraConsumptionTypeId, s.FiderConsumptionTypeId, s.ReceivedEnergyId }).Select(s => s.FirstOrDefault()).ToList();


            //foreach (var itm in tms)
            //{
            //    var currenttrs = trs.Where(s => s.TransformerCenterId == itm.Id && s.changed == false).ToList();
            //    itm.Id = 0;
            //    TransformerCenter.SaveTransformerCenter(itm);
            //    foreach (var trx in currenttrs)
            //    {
            //        trx.TransformerCenterId = itm.Id;
            //        trx.changed = true;
            //    }
            //}
            //foreach (var tmp in trs.Where(s => s.changed == false))
            //    tmp.TransformerCenterId = 0;
            //foreach (var itm in trs)
            //{
            //    Transformer.SaveTransformer(itm);
            //}
        var    trs = Transformer.ListTransformer(0, 1000000, null).ToList<EdwTransformer>(null);
            //tms = TransformerCenter.ListTransformerCenter(0, 100000, null).ToList<EdwTransformerCenter>(null);
            foreach (var itm in shs)
            {
                if (string.IsNullOrEmpty(itm.TransformerName))
                    throw new Exception("trafo adı boş");
                var tr2 = trs.Where(s => s.Name.TrimStart().TrimEnd() == itm.TransformerName.TrimStart().TrimEnd() && s.TransformerCenterName.TrimStart().TrimEnd() == itm.TransformerCenterName.TrimStart().TrimEnd() && s.ReceivedEnergyId == itm.ReceivedEnergyId);
                if (tr2.Count() > 1)
                {
                    throw new Exception("tekrarlı kayıt");
                }
                var tr = tr2.FirstOrDefault();
                if (tr == null)
                    throw new Exception("trafo bulunamadı");
                itm.TransformerId = tr.Id;
                itm.TransformerCenterId = tr.TransformerCenterId;
                StatusHistory.SaveStatusHistory(itm);
            }
        }
    }
}