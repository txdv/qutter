using System;
using System.Collections.Generic;

namespace Qutter.App
{
	public class Identity
	{
		public Network Network { get; internal set; }

		public int Id { get; internal set; }
		public string RealName { get; internal set; }
		public string QuitReason { get; internal set; }
		public string PartReason { get; internal set; }
		public string[] Nicks { get; internal set; }
		public string KickReason { get; internal set; }
		public string Name { get; internal set; }
		public string Ident { get; internal set; }
		public bool DetachAwayReasonEnabled { get; internal set; }
		public string DetachAwayReason { get; internal set; }
		public bool DetachAwayEnabled { get; internal set; }
		public bool AwayReasonEnabled { get; internal set; }
		public string AwayReason { get; internal set; }
		public bool AwayNickEnabled { get; internal set; }
		public string AwayNick { get; internal set; }
		public int AutoAwayTime { get; internal set; }
		public bool AutoAwayReasonEnabled { get; internal set; }
		public string AutoAwayReason { get; internal set; }
		public bool AutoAwayEnabled { get; internal set; }

		public Identity()
		{
		}
	}

	public class IdentitySerializer : QMetaTypeSerializer<Identity>
	{
		public void Serialize (MiscUtil.IO.EndianBinaryWriter bw, Identity data)
		{
		}

		public Identity Deserialize(MiscUtil.IO.EndianBinaryReader br, Type type)
		{
			var identity = new Identity();
			Dictionary<string, QVariant> map;
			QTypeManager.Deserialize<Dictionary<string, QVariant>>(br.BaseStream, out map);
			identity.RealName = map["realName"].Value as string;
			identity.QuitReason = map["quitReason"].Value as string;
			identity.PartReason = map["partReason"].Value as string;
			identity.Nicks = map["nicks"].GetValue<List<string>>().ToArray();
			identity.KickReason = map["kickReason"].Value as string;
			identity.Name = map["identityName"].Value as string;
			identity.Id = map["identityId"].GetValue<int>();
			identity.Ident = map["ident"].Value as string;
			identity.DetachAwayReasonEnabled = map["detachAwayReasonEnabled"].GetValue<bool>();
			identity.DetachAwayReason = map["detachAwayReason"].Value as string;
			identity.DetachAwayEnabled = map["detachAwayEnabled"].GetValue<bool>();
			identity.AwayReasonEnabled = map["awayReasonEnabled"].GetValue<bool>();
			identity.AwayReason = map["awayReason"].Value as string;
			identity.AwayNickEnabled = map["awayNickEnabled"].GetValue<bool>();
			identity.AwayNick = map["awayNick"].Value as string;
			identity.AutoAwayTime = map["autoAwayTime"].GetValue<int>();
			identity.AutoAwayReasonEnabled = map["autoAwayReasonEnabled"].GetValue<bool>();
			identity.AutoAwayReason = map["autoAwayReason"].Value as string;
			identity.AutoAwayEnabled = map["autoAwayEnabled"].GetValue<bool>();
			return identity;
		}
	}
}

