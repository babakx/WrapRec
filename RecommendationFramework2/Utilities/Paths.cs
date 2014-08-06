using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2.Utilities
{
    public class Paths
    {
        // MovieLens
        public static string MovieLens1M = @"F:\Data\Datasets\MovieLens\ml-1m\ratings.dat";
        public static string MovieLens1MTrain75 = @"F:\Data\Datasets\MovieLens\ml-1m\ratings75.dat";
        public static string MovieLens1MTest25 = @"F:\Data\Datasets\MovieLens\ml-1m\ratings25.dat";
        public static string MovieLens1MUsersCluster = @"F:\Data\Datasets\MovieLens\ml-1m\Clusters\UsersCluster";
        public static string MovieLens1MItemsCluster = @"F:\Data\Datasets\MovieLens\ml-1m\Clusters\ItemsCluster";
        public static string MovieLens1MUsersRatingsCount = @"F:\Data\Datasets\MovieLens\ml-1m\UsersRatingsCount.csv";
        public static string MovieLens1MItemsRatingsCount = @"F:\Data\Datasets\MovieLens\ml-1m\ItemsRatingsCount.csv";

        // Epinion
        public static string EpinionRatings = @"F:\Data\Datasets\Epinion\ratings_data.txt";
        public static string EpinionTrain75 = @"F:\Data\Datasets\Epinion\ratings_data_75.txt";
        public static string EpinionTest25 = @"F:\Data\Datasets\Epinion\ratings_data_25.txt";
        public static string EpinionItemsCluster = @"F:\Data\Datasets\Epinion\Clusters\ItemsCluster";
        public static string EpinionUsersCluster = @"F:\Data\Datasets\Epinion\Clusters\UsersCluster";

        public static string EpinionTrain80 = @"F:\Data\Dropbox\TUDelft joint paper\data\Epinion\ratings_train_80.txt";
        public static string EpinionTest20 = @"F:\Data\Dropbox\TUDelft joint paper\data\Epinion\ratings_test_20.txt";
        public static string EpinionTrain50 = @"F:\Data\Dropbox\TUDelft joint paper\data\Epinion\ratings_train_50.txt";
        public static string EpinionTest50 = @"F:\Data\Dropbox\TUDelft joint paper\data\Epinion\ratings_test_50.txt";
        public static string EpinionRelations = @"F:\Data\Dropbox\TUDelft joint paper\data\Epinion\trust_data.txt";
        public static string EpinionRelationsImplicit = @"F:\Data\Dropbox\TUDelft joint paper\data\trust scores\";

        //Amazon
        public static string AmazonBooksRatings = @"F:\Data\Datasets\Amazon\Selected\books_selected.csv";
        public static string AmazonMusicRatings = @"F:\Data\Datasets\Amazon\Selected\music_selected.csv";
        public static string AmazonDvdRatings = @"F:\Data\Datasets\Amazon\Selected\dvd_selected.csv";
        public static string AmazonVideoRatings = @"F:\Data\Datasets\Amazon\Selected\video_selected.csv";

        public static string AmazonBooksUsersCluster = @"F:\Data\Datasets\Amazon\Selected\BookClusters\UsersCluster";
        public static string AmazonBooksItemsCluster = @"F:\Data\Datasets\Amazon\Selected\BookClusters\ItemsCluster";

        public static string AmazonMusicUsersCluster = @"F:\Data\Datasets\Amazon\Selected\MusicClusters\UsersCluster";
        public static string AmazonMusicItemsCluster = @"F:\Data\Datasets\Amazon\Selected\MusicClusters\ItemsCluster";

        public static string AmazonVideoUsersCluster = @"F:\Data\Datasets\Amazon\Selected\VideoClusters\UsersCluster";
        public static string AmazonVideoItemsCluster = @"F:\Data\Datasets\Amazon\Selected\VideoClusters\ItemsCluster";

        public static string AmazonDvdUsersCluster = @"F:\Data\Datasets\Amazon\Selected\DvdClusters\UsersCluster";
        public static string AmazonDvdItemsCluster = @"F:\Data\Datasets\Amazon\Selected\DvdClusters\ItemsCluster";

        public static string AmazonBooksTrain75 = @"F:\Data\Datasets\Amazon\Selected\books_selected_75.csv";
        public static string AmazonBooksTest25 = @"F:\Data\Datasets\Amazon\Selected\books_selected_25.csv";

        public static string AmazonMusicTrain75 = @"F:\Data\Datasets\Amazon\Selected\music_selected_75.csv";
        public static string AmazonMusicTest25 = @"F:\Data\Datasets\Amazon\Selected\music_selected_25.csv";

    }
}
