using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CStarter.OptionsSharp
{
    public abstract class Option
    {
        string prototype, description;
        string[] names;
        OptionValueType type;
        int count;
        string[] separators;

        protected Option(string prototype, string description)
            : this(prototype, description, 1)
        {
        }

        protected Option(string prototype, string description, int maxValueCount)
        {
            if (prototype == null)
                throw new ArgumentNullException("prototype");
            if (prototype.Length == 0)
                throw new ArgumentException("不能为空字符串", "prototype");
            if (maxValueCount < 0)
                throw new ArgumentOutOfRangeException("maxValueCount");

            this.prototype = prototype;
            this.names = prototype.Split('|');
            this.description = description;
            this.count = maxValueCount;
            this.type = ParsePrototype();

            if (this.count == 0 && type != OptionValueType.None)
                throw new ArgumentException(
                        "OptionValueType 为 OptionValueType.Required 或者 " +
                            "OptionValueType.Optional 时， maxValueCount不能为零",
                        "maxValueCount");
            if (this.type == OptionValueType.None && maxValueCount > 1)
                throw new ArgumentException(
                        string.Format("当 OptionValueType 为 OptionValueType.None 时， maxValueCount 不能为 {0}", maxValueCount),
                        "maxValueCount");
            if (Array.IndexOf(names, "<>") >= 0 &&
                    ((names.Length == 1 && this.type != OptionValueType.None) ||
                     (names.Length > 1 && this.MaxValueCount > 1)))
                throw new ArgumentException(
                        "对'<>'的处理不能包含值。",
                        "prototype");
        }

        public string Prototype { get { return prototype; } }
        public string Description { get { return description; } }
        public OptionValueType OptionValueType { get { return type; } }
        public int MaxValueCount { get { return count; } }

        public string[] GetNames()
        {
            return (string[])names.Clone();
        }

        public string[] GetValueSeparators()
        {
            if (separators == null)
                return new string[0];
            return (string[])separators.Clone();
        }

        protected static T Parse<T>(string value, OptionContext c)
        {
            TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
            T t = default(T);
            try
            {
                if (value != null)
                    t = (T)conv.ConvertFromString(value);
            }
            catch (Exception e)
            {
                throw new OptionException(
                        string.Format(
                            c.OptionSet.MessageLocalizer("无法把参数中的`{2}' 字符串`{0}' 转换为 {1}"),
                            value, typeof(T).Name, c.OptionName),
                        c.OptionName, e);
            }
            return t;
        }

        internal string[] Names { get { return names; } }
        internal string[] ValueSeparators { get { return separators; } }

        static readonly char[] NameTerminator = new char[] { '=', ':' };

        private OptionValueType ParsePrototype()
        {
            char type = '\0';
            List<string> seps = new List<string>();
            for (int i = 0; i < names.Length; ++i)
            {
                string name = names[i];
                if (name.Length == 0)
                    throw new ArgumentException("不支持空的参数名", "prototype");

                int end = name.IndexOfAny(NameTerminator);
                if (end == -1)
                    continue;
                names[i] = name.Substring(0, end);
                if (type == '\0' || type == name[end])
                    type = name[end];
                else
                    throw new ArgumentException(
                            string.Format("参数类型冲突: '{0}' vs. '{1}'.", type, name[end]),
                            "prototype");
                AddSeparators(name, end, seps);
            }

            if (type == '\0')
                return OptionValueType.None;

            if (count <= 1 && seps.Count != 0)
                throw new ArgumentException(
                        string.Format("参数无法解析为键值类型 {0} value(s).", count),
                        "prototype");
            if (count > 1)
            {
                if (seps.Count == 0)
                    this.separators = new string[] { ":", "=" };
                else if (seps.Count == 1 && seps[0].Length == 0)
                    this.separators = null;
                else
                    this.separators = seps.ToArray();
            }

            return type == '=' ? OptionValueType.Required : OptionValueType.Optional;
        }

        private static void AddSeparators(string name, int end, ICollection<string> seps)
        {
            int start = -1;
            for (int i = end + 1; i < name.Length; ++i)
            {
                switch (name[i])
                {
                    case '{':
                        if (start != -1)
                            throw new ArgumentException(
                                    string.Format("在 \"{0}\" 中包含错误的name/value分隔符", name),
                                    "prototype");
                        start = i + 1;
                        break;
                    case '}':
                        if (start == -1)
                            throw new ArgumentException(
                                    string.Format("在 \"{0}\" 中包含错误的name/value分隔符", name),
                                    "prototype");
                        seps.Add(name.Substring(start, i - start));
                        start = -1;
                        break;
                    default:
                        if (start == -1)
                            seps.Add(name[i].ToString());
                        break;
                }
            }
            if (start != -1)
                throw new ArgumentException(
                        string.Format("在 \"{0}\" 中包含错误的name/value分隔符", name),
                        "prototype");
        }

        public void Invoke(OptionContext c)
        {
            OnParseComplete(c);
            c.OptionName = null;
            c.Option = null;
            c.OptionValues.Clear();
        }

        protected abstract void OnParseComplete(OptionContext c);

        public override string ToString()
        {
            return Prototype;
        }
    }
}
