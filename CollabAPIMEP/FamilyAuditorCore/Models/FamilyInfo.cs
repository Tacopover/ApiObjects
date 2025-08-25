using System.Collections.Generic;

namespace FamilyAuditorCore.Models
{
    /// <summary>
    /// Represents family information extracted from a CAD family document,
    /// without any direct dependencies on CAD API objects.
    /// This abstracted model allows for validation without requiring specific CAD software to be installed.
    /// </summary>
    public class FamilyInfo
    {
        /// <summary>
        /// Gets or sets the file path of the family file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the number of parameters in the family
        /// </summary>
        public int ParameterCount { get; set; }

        /// <summary>
        /// Gets or sets the number of geometric elements in the family
        /// </summary>
        public int ElementCount { get; set; }

        /// <summary>
        /// Gets or sets the number of imported instances (external references) in the family
        /// </summary>
        public int ImportedInstanceCount { get; set; }

        /// <summary>
        /// Gets or sets the number of materials used in the family
        /// </summary>
        public int MaterialCount { get; set; }

        /// <summary>
        /// Gets or sets the number of detail lines (symbolic lines) in the family
        /// </summary>
        public int DetailLineCount { get; set; }

        /// <summary>
        /// Gets or sets the number of vertices in the family geometry
        /// </summary>
        public int VertexCount { get; set; }

        /// <summary>
        /// Gets or sets whether the family contains elements without subcategory assignments
        /// </summary>
        public bool HasElementsWithoutSubcategory { get; set; }
        
        /// <summary>
        /// Gets or sets additional custom properties that can be checked against rules
        /// This allows for extensibility without modifying the core model
        /// </summary>
        public Dictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();
    }
}