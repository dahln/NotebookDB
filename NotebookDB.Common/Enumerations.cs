using System.ComponentModel;
using System.Reflection;

namespace NotebookDB.Common.Enumerations
{
    public enum Gender
    {
        [Description("Not Specified")]
        NotSpecified,

        [Description("Male")]
        Male,

        [Description("Female")]
        Female
    }

    public enum FieldType
    {
        Text = 1,
        TextArea = 2,
        Number = 3,
        Date = 4,
        List = 7,
        Label = 8,
        Image = 9,
        URL = 10,
        Money = 11,
        Decimal = 12
    }

    public static class EnumExtensionMethods
    {
        public static string GetDescription(this Enum GenericEnum)
        {
            Type genericEnumType = GenericEnum.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0))
            {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if ((_Attribs != null && _Attribs.Count() > 0))
                {
                    return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
                }
            }
            return GenericEnum.ToString();
        }
    }
}
