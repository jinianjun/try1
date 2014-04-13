using System;
using System.Collections.Generic;
using System.Text;

namespace AppBase
{
    public class SqlHelper
    {
        /// <summary>
        /// 计划写一个分页SQL
        /// 目的：整合系统的时候，可以不用在目标数据库中加入分页存储过程。
        /// 实现方法：改造分页存储过程
        /// </summary>
        /// <param name="pageListObject"></param>
        /// <returns></returns>
        //public string getPageListSqlString(PageListObject pageListObject)
        //{
        //}
    }

    public class PageListObject
    {
        //@tblName     nvarchar(200),        ----要显示的表或多个表的连接
        //@fldName     nvarchar(500) = '*',    ----要显示的字段列表
        //@pageSize    int = 1,        ----每页显示的记录个数
        //@page        int = 10,        ----要显示那一页的记录
        //@fldSort    nvarchar(200) = null,    ----排序字段列表或条件
        //@Sort        bit = 0,        ----排序方法，0为升序，1为降序(如果是多字段排列Sort指代最后一个排序字段的排列顺序(最后一个排序字段不加排序标记)--程序传参如：' SortA Asc,SortB Desc,SortC ')
        //@strCondition    nvarchar(1000) = null,    ----查询条件,不需where,以And开始
        //@ID        nvarchar(150),        ----主表的主键
        //@Dist                 bit = 0 ,          ----是否添加查询字段的 DISTINCT 默认0不添加/1添加
        //@pageCount    int = 1 output,            ----查询结果分页后的总页数
        //@Counts    int = 1 output,                ----查询到的记录数
        //@strSql          nvarchar(1000) = '' output  -----最后返回的SQL语句

        string FromTables { get; set; }
        string SelectFields { get; set; }
        int PageSize { get; set; }
        int PageIndex { get; set; }
        string FieldsSort { get; set; }
        int Sort { get; set; }
        string strCondition { get; set; }
        string ID { get; set; }
        int Dist { get; set; }
        int PageCount { get; set; }
        int Counts { get; set; }
        string strSql { get; set; }
    }
}
