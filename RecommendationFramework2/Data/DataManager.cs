using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Samples.EntityDataReader;
using System.Data.SqlClient;
using System.Configuration;
using WrapRec.Readers;
using CsvHelper.Configuration;
using System.IO;
using WrapRec.Data.DatasetImporters;
using WrapRec.Utilities;

namespace WrapRec.Data
{
    public class DataManager
    {
        public RecSysContext RecSysContext { get; set; }
        IList<IDatasetImporter> _importers;

        public DataManager()
        {
            RecSysContext = new RecSysContext();
            _importers = new List<IDatasetImporter>();
        }

        public static RecSysContext GetDataContext()
        {
            Console.WriteLine("Preparing data context...");

            var dm = new DataManager();

            dm.AddImporter(new MovieLensImporter("MovieLens1M", Paths.MovieLens1M));
            dm.AddImporter(new EpinionImporter(Paths.EpinionRatings, Paths.EpinionRelations));

            dm.ImportDatasets();

            return dm.RecSysContext;
        }

        public void AddImporter(IDatasetImporter importer)
        {
            _importers.Add(importer);
        }

        public void ImportDatasets()
        {
            foreach (var importer in _importers)
            {
                string datasetName = importer.GetDatasetName();

                // in case the dataset with same name was imported before, no import is done
                if (RecSysContext.Datasets.Any(d => string.Equals(datasetName.ToLower(), d.Name.ToLower())))
                {
                    Console.WriteLine("Dataset with name {0} already imported.", datasetName);
                }
                else
                {
                    Console.WriteLine("Importing dataset {0} ...", datasetName);
                    var dataset = AddDatasetRecord(datasetName);

                    try
                    {
                        importer.ImportData(RecSysContext, dataset);
                        Console.WriteLine("Dataset {0} imported succesfully.", datasetName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to import dataset {0} with the following error: \n {1}", datasetName, ex.Message);
                        RemoveDatasetRecord(dataset);
                    }

                }
            }
        }

        private Dataset AddDatasetRecord(string datasetName)
        {
            Dataset dataset = new Dataset();
            dataset.Name = datasetName;

            var newDataset = RecSysContext.Datasets.Add(dataset);
            RecSysContext.SaveChanges();

            return newDataset;
        }

        private void RemoveDatasetRecord(Dataset dataset)
        {
            RecSysContext.Datasets.Remove(dataset);
            RecSysContext.SaveChanges();
        }
        
    }
}
