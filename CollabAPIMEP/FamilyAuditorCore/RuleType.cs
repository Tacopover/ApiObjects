using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyAuditorCore
{
    /// <summary>
    /// Defines the types of validation rules that can be applied to families
    /// Each rule type corresponds to a specific aspect of family quality and compliance
    /// </summary>
    public enum RuleType
    {
        /// <summary>
        /// Validates the number of geometric elements in the family
        /// Helps prevent overly complex families that could impact performance
        /// </summary>
        NumberOfElements,

        /// <summary>
        /// Validates the number of imported instances (external references) in the family
        /// Imported geometry can cause performance issues and should generally be avoided
        /// </summary>
        ImportedInstances,

        /// <summary>
        /// Validates that all geometric elements are assigned to appropriate subcategories
        /// Proper subcategory assignment is essential for visibility control and standards compliance
        /// </summary>
        SubCategory,

        /// <summary>
        /// Validates the number of materials used in the family
        /// Excessive materials can complicate material management and impact performance
        /// </summary>
        Material,

        /// <summary>
        /// Validates the number of parameters in the family
        /// Too many parameters can make families difficult to use and maintain
        /// </summary>
        NumberOfParameters,

        /// <summary>
        /// Validates the file size of the family
        /// Large family files can impact project performance and loading times
        /// </summary>
        FileSize,

        /// <summary>
        /// Validates the number of detail lines (symbolic/annotation lines) in the family
        /// Excessive detail lines can impact performance in views
        /// </summary>
        DetailLines,

        /// <summary>
        /// Validates the number of vertices in the family geometry
        /// High vertex counts indicate complex geometry that may impact performance
        /// </summary>
        Vertices
    }
}
