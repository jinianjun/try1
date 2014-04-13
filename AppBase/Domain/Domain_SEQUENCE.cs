using System;
namespace AppBase.Domain
{
	[Serializable]
	public class Domain_SEQUENCE
	{
		#region 私有成员
			
		private bool m_IsChanged;
		private bool m_IsDeleted;
		private string m_TABLE_NAME; 
		private int m_NEXT_ID; 		

		#endregion
		
		#region 默认( 空 ) 构造函数
		/// <summary>
		/// 默认构造函数
		/// </summary>
		public Domain_SEQUENCE()
		{
			m_TABLE_NAME = null; 
			m_NEXT_ID = 0; 
		}
		#endregion
		
		#region 公有属性
			
		/// <summary>
		/// 
		/// </summary>		
		public virtual string TABLE_NAME
		{
			get { return m_TABLE_NAME; }
			set	
			{
				if ( value != null)
					if( value.Length > 50)
						throw new ArgumentOutOfRangeException("Invalid value for TABLE_NAME", value, value.ToString());
				
				m_IsChanged |= (m_TABLE_NAME != value); m_TABLE_NAME = value;
			}
		}
			
		/// <summary>
		/// 
		/// </summary>		
		public virtual int NEXT_ID
		{
			get { return m_NEXT_ID; }
			set { m_IsChanged |= (m_NEXT_ID != value); m_NEXT_ID = value; }
		}
			
		/// <summary>
		/// 对象的值是否被改变
		/// 按1.2的要求已经加上virtual
		/// </summary>
		public virtual bool IsChanged
		{
			get { return m_IsChanged; }
		}
		
		/// <summary>
		/// 对象是否已经被删除
		/// 按1.2的要求已经加上virtual
		/// </summary>
		public virtual bool IsDeleted
		{
			get { return m_IsDeleted; }
		}
		
		#endregion 
		
		#region 公有函数
		
		/// <summary>
		/// 标记对象已删除
		/// 按1.2的要求已经加上virtual
		/// </summary>
		public virtual void MarkAsDeleted()
		{
			m_IsDeleted = true;
			m_IsChanged = true;
		}
		
		
		#endregion
		
		#region 重写Equals和HashCode
		/// <summary>
		/// 用唯一值实现Equals
		/// </summary>
		public override bool Equals( object obj )
		{
			if( this == obj ) return true;
			if( ( obj == null ) || ( obj.GetType() != GetType() ) ) return false;
			Domain_SEQUENCE castObj = (Domain_SEQUENCE)obj; 
			return ( castObj != null ) &&
				(m_TABLE_NAME == castObj.TABLE_NAME );
		}
		
		/// <summary>
		/// 用唯一值实现GetHashCode
		/// </summary>
		public override int GetHashCode()
		{
			int hash = 57; 
			hash = 27 * hash * m_TABLE_NAME.GetHashCode();
			return hash; 
		}
		#endregion
		
	}
}
