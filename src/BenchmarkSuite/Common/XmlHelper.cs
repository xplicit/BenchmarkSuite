﻿using System;
using System.Xml;
using System.Globalization;

namespace BenchmarkSuite.Common
{
    public static class XmlHelper
    {
        /// <summary>
        /// Creates a new top level element node.
        /// </summary>
        /// <param name="name">The element name.</param>
        /// <returns>A new XmlNode</returns>
        public static XmlNode CreateTopLevelElement(string name)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml( "<" + name + "/>" );
            return doc.FirstChild;
        }

        /// <summary>
        /// Adds an attribute with a specified name and value to an existing XmlNode.
        /// </summary>
        /// <param name="node">The node to which the attribute should be added.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public static void AddAttribute(this XmlNode node, string name, string value)
        {
            XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
            attr.Value = value;
            node.Attributes.Append(attr);
        }

        /// <summary>
        /// Adds a new element as a child of an existing XmlNode and returns it.
        /// </summary>
        /// <param name="node">The node to which the element should be added.</param>
        /// <param name="name">The element name.</param>
        /// <returns>The newly created child element</returns>
        public static XmlNode AddElement(this XmlNode node, string name)
        {
            XmlNode childNode = node.OwnerDocument.CreateElement(name);
            node.AppendChild(childNode);
            return childNode;
        }

        #region Safe Attribute Access

        /// <summary>
        /// Gets the value of the given attribute.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string GetAttribute(this XmlNode result, string name)
        {
            XmlAttribute attr = result.Attributes[name];

            return attr == null ? null : attr.Value;
        }

        /// <summary>
        /// Gets the value of the given attribute as an int.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int GetAttribute(this XmlNode result, string name, int defaultValue)
        {
            XmlAttribute attr = result.Attributes[name];

            return attr == null
                ? defaultValue
                    : int.Parse(attr.Value, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the value of the given attribute as a double.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static double GetAttribute(this XmlNode result, string name, double defaultValue)
        {
            XmlAttribute attr = result.Attributes[name];

            return attr == null
                ? defaultValue
                    : double.Parse(attr.Value, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the value of the given attribute as a DateTime.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static DateTime GetAttribute(this XmlNode result, string name, DateTime defaultValue)
        {
            string dateStr = GetAttribute(result, name);
            if (dateStr == null)
                return defaultValue;

            DateTime date;
            if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AllowWhiteSpaces, out date))
                return defaultValue;

            return date;
        }

        #endregion

    }
}

