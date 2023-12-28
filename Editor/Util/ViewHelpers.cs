using System.Collections.Generic;

namespace RenderPipelineGraph.Editor
{
    static class ViewHelpers
    {
        public static string MakeNameUnique(string name, HashSet<string> allNames)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "parameter";
            }
            string candidateName = name.Trim();
            if (candidateName.Length < 1)
            {
                return null;
            }
            string candidateMainPart = null;
            int cpt = 0;
            while (allNames.Contains(candidateName))
            {
                if (candidateMainPart == null)
                {
                    int spaceIndex = candidateName.LastIndexOf(' ');
                    if (spaceIndex == -1)
                    {
                        candidateMainPart = candidateName;
                    }
                    else
                    {
                        if (int.TryParse(candidateName.Substring(spaceIndex + 1), out cpt)) // spaceIndex can't be last char because of Trim()
                        {
                            candidateMainPart = candidateName.Substring(0, spaceIndex);
                        }
                        else
                        {
                            candidateMainPart = candidateName;
                        }
                    }
                }
                ++cpt;

                candidateName = string.Format("{0} {1}", candidateMainPart, cpt);
            }

            return candidateName;
        }
    }
}
