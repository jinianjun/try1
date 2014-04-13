using System;
using System.Collections.Generic;
using System.Text;
using AppBase.Domain;

namespace AppBase.Sequence
{
    public class SequenceService : BaseService<Domain_SEQUENCE>, ISequenceService
    {
        protected override IBaseDao<Domain_SEQUENCE> BaseDao { get; set; }

        #region ISequenceService<Domain_SEQUENCE> Members

        /// <summary>
        /// 方法原型来自：
        /// PermissionBase/Core/Service/IdGen.cs
        /// 原方法有镜象服务支持，这里没有加
        /// </summary>

        public int GetNextId(string tablename)
        {
            //对tablename进行有效性检查(可以说是多余的)
            if (string.IsNullOrEmpty(tablename)) throw new ArgumentNullException("tablename");
            
            int currentId = 0;
            Domain_SEQUENCE sequence;

            sequence = BaseDao.Get(tablename, NHibernate.LockMode.Upgrade); //20091203在没有记录的情况下进行锁定，不会报错

            if (sequence == null)
            {
                sequence = new Domain_SEQUENCE();
                sequence.TABLE_NAME = tablename;
                sequence.NEXT_ID = currentId;
                BaseDao.Save(sequence);
                //新增记录用于锁定
                sequence = BaseDao.Get(tablename, NHibernate.LockMode.Upgrade);
            }

            currentId = sequence.NEXT_ID + 1;
            //返回值存入数据库，实现同步
            sequence.NEXT_ID = currentId;
            BaseDao.Update(sequence);

            return currentId;
        }

        #endregion
    }
}
