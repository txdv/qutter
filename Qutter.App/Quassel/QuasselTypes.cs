using System;

namespace Qutter.App
{
	public static class QuasselTypes
	{
		public static void Init()
		{
			QTypeManager.Register("MsgId",      typeof(IdSerializer));
			QTypeManager.Register("BufferId",   typeof(IdSerializer));
			QTypeManager.Register("NetworkId",  typeof(IdSerializer));
			QTypeManager.Register("Identity",   typeof(IdentitySerializer));
			QTypeManager.Register("IdentityId", typeof(IdSerializer));
			QTypeManager.Register("Message",    typeof(MessageSerializer));
			QTypeManager.Register("BufferInfo", typeof(BufferInfoSerializer));
		}
	}
}

