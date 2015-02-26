using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Utilities
{
    public class Paths
    {
        // MovieLens
        public static string MovieLens1M = @"D:\Data\Datasets\MovieLens\ml-1m\ratings.dat";
        public static string MovieLens1MMovies = @"D:\Data\Datasets\MovieLens\ml-1m\movies.dat";
        public static string MovieLens1MTrain75 = @"D:\Data\Datasets\MovieLens\ml-1m\ratings75.dat";
        public static string MovieLens1MTest25 = @"D:\Data\Datasets\MovieLens\ml-1m\ratings25.dat";
        public static string MovieLens1MUsersCluster = @"D:\Data\Datasets\MovieLens\ml-1m\Clusters\UsersCluster";
        public static string MovieLens1MItemsCluster = @"D:\Data\Datasets\MovieLens\ml-1m\Clusters\ItemsCluster";
        public static string MovieLens1MUsersRatingsCount = @"D:\Data\Datasets\MovieLens\ml-1m\UsersRatingsCount.csv";
        public static string MovieLens1MItemsRatingsCount = @"D:\Data\Datasets\MovieLens\ml-1m\ItemsRatingsCount.csv";

        // Epinion
        public static string EpinionRoot = @"D:\Data\Datasets\Epinion\";
        public static string EpinionRatings = @"D:\Data\Datasets\Epinion\ratings_data.txt";
        public static string EpinionTrain75 = @"D:\Data\Datasets\Epinion\ratings_data_75.txt";
        public static string EpinionTest25 = @"D:\Data\Datasets\Epinion\ratings_data_25.txt";
        public static string EpinionItemsCluster = @"D:\Data\Datasets\Epinion\Clusters\ItemsCluster";
        public static string EpinionUsersCluster = @"D:\Data\Datasets\Epinion\Clusters\UsersCluster";

        public static string EpinionTrain80 = @"D:\Data\Datasets\Epinion\ratings_train_80.txt";
        public static string EpinionTest20 = @"D:\Data\Datasets\Epinion\ratings_test_20.txt";
        public static string EpinionTrain50 = @"D:\Data\Dropbox\TUDelft joint paper\data\Epinion\ratings_train_50.txt";
        public static string EpinionTest50 = @"D:\Data\Dropbox\TUDelft joint paper\data\Epinion\ratings_test_50.txt";
        public static string EpinionRelations = @"D:\Data\Datasets\Epinion\trust_data.txt";
        public static string EpinionRelationsImplicit = @"D:\Data\Datasets\Epinion\epinion-trust_values\trust_values_PearsonTrustMetric_0.707_2.dat";

        // Amazon
        public static string AmazonBooksRatings = @"D:\Data\Datasets\Amazon\Selected\books_selected.csv";
        public static string AmazonMusicRatings = @"D:\Data\Datasets\Amazon\Selected\music_selected.csv";
        public static string AmazonDvdRatings = @"D:\Data\Datasets\Amazon\Selected\dvd_selected.csv";
        public static string AmazonVideoRatings = @"D:\Data\Datasets\Amazon\Selected\video_selected.csv";

        //public static string AmazonBooksRatings = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle1.csv";
        //public static string AmazonMusicRatings = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle2.csv";
        //public static string AmazonDvdRatings = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle3.csv";
        //public static string AmazonVideoRatings = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle4.csv";


        public static string AmazonShuffle1 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle1.csv";
        public static string AmazonShuffle2 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle2.csv";
        public static string AmazonShuffle3 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle3.csv";
        public static string AmazonShuffle4 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle4.csv";


        public static string AmazonBooksUsersCluster = @"D:\Data\Datasets\Amazon\Selected\BookClusters\UsersCluster";
        public static string AmazonBooksItemsCluster = @"D:\Data\Datasets\Amazon\Selected\BookClusters\ItemsCluster";

        public static string AmazonMusicUsersCluster = @"D:\Data\Datasets\Amazon\Selected\MusicClusters\UsersCluster";
        public static string AmazonMusicItemsCluster = @"D:\Data\Datasets\Amazon\Selected\MusicClusters\ItemsCluster";

        public static string AmazonVideoUsersCluster = @"D:\Data\Datasets\Amazon\Selected\VideoClusters\UsersCluster";
        public static string AmazonVideoItemsCluster = @"D:\Data\Datasets\Amazon\Selected\VideoClusters\ItemsCluster";

        public static string AmazonDvdUsersCluster = @"D:\Data\Datasets\Amazon\Selected\DvdClusters\UsersCluster";
        public static string AmazonDvdItemsCluster = @"D:\Data\Datasets\Amazon\Selected\DvdClusters\ItemsCluster";

        public static string AmazonBooksTrain75 = @"D:\Data\Datasets\Amazon\Selected\books_selected_75.csv";
        public static string AmazonBooksTest25 = @"D:\Data\Datasets\Amazon\Selected\books_selected_25.csv";

        public static string AmazonMusicTrain75 = @"D:\Data\Datasets\Amazon\Selected\music_selected_75.csv";
        public static string AmazonMusicTest25 = @"D:\Data\Datasets\Amazon\Selected\music_selected_25.csv";

        public static string AmazonDvdTrain75 = @"D:\Data\Datasets\Amazon\Selected\dvd_selected_75.csv";
        public static string AmazonDvdTest25 = @"D:\Data\Datasets\Amazon\Selected\dvd_selected_25.csv";

        public static string AmazonVideoTrain75 = @"D:\Data\Datasets\Amazon\Selected\video_selected_75.csv";
        public static string AmazonVideoTest25 = @"D:\Data\Datasets\Amazon\Selected\video_selected_25.csv";

        //public static string AmazonBooksTrain75 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle1.csv.75";
        //public static string AmazonBooksTest25 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle1.csv.25";

        //public static string AmazonMusicTrain75 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle2.csv.75";
        //public static string AmazonMusicTest25 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle2.csv.25";

        //public static string AmazonDvdTrain75 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle3.csv.75";
        //public static string AmazonDvdTest25 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle3.csv.25";

        //public static string AmazonVideoTrain75 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle4.csv.75";
        //public static string AmazonVideoTest25 = @"D:\Data\Datasets\Amazon\Selected\Shuffles\shuffle4.csv.25";


        public static string AmazonAllBookRatings = @"D:\Data\Datasets\Amazon\Processed\AllBooksRatings.csv";
        public static string AmazonAllMusicRatings = @"D:\Data\Datasets\Amazon\Processed\AllMusicsRatings.csv";
        public static string AmazonAllDvdRatings = @"D:\Data\Datasets\Amazon\Processed\AllDvdRatings.csv";
        public static string AmazonAllVideoRatings = @"D:\Data\Datasets\Amazon\Processed\AllVideoRatings.csv";


        public static string AmazonProcessedPath = @"D:\Data\Datasets\Amazon\Processed";

        // Test
        public static string TestDomain1Train = @"D:\Data\Datasets\Amazon\Test\Domain1.csv";
        public static string TestDomain1Test = @"D:\Data\Datasets\Amazon\Test\Domain1_test.csv";
        public static string TestDomain2 = @"D:\Data\Datasets\Amazon\Test\Domain2.csv";

        // CrowdRec data
        public static string MovieLensCrowdRecEntities = @"D:\Data\Datasets\MovieLens\CrowdRecFormat\entities.dat";
        public static string MovieLensCrowdRecRelations = @"D:\Data\Datasets\MovieLens\CrowdRecFormat\relations.dat";

    }
}
