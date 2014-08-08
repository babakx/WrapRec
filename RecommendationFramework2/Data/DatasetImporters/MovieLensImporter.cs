using RF2.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2.Data.DatasetImporters
{
    public class MovieLensImporter : IDatasetImporter
    {
        string _datasetName, _datasetPath;

        public MovieLensImporter(string datasetName, string datasetPath)
        {
            _datasetName = datasetName;
            _datasetPath = datasetPath;
        }
        
        public void ImportData(RecSysContext recSysContext, Dataset datasetRecord)
        {
            var itemRatings = (new MovieLensReader(_datasetPath)).ReadAll();
            recSysContext.ImportItemRatings(itemRatings, datasetRecord);
        }

        public string GetDatasetName()
        {
            return _datasetName;
        }
    }
}
