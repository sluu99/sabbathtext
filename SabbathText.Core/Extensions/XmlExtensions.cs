using System.Xml;

public static class XmlExtensions
{
    public static string SelectNodeInnerText(this XmlNode node, string xpath, string defaultValue)
    {
        XmlNode innerNode = node.SelectSingleNode(xpath);
        if (innerNode == null)
        {
            return defaultValue;
        }

        return innerNode.InnerText.Trim();
    }
}
