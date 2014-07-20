/*
 * Author:  Seth Waxman
 * Date:    June 19th, 2014
 * 
 * Intent:  I was speaking with a team in New York City who requested I show an example implementation
 *          of K-Means CLustering, a very common clustering technique often used in explatory analysis.
 *          As this group is primarily financial in nature, I have decided to show how K-Means Clustering
 *          can be used to identify outliers, which is a possible indication of fraud. This example uses dummy
 *          data over 30 investors and tries to correlate them into clustered groups.
 */

using System;

namespace KMeans2
{
    class KMeans2
    {

        static void Main(string[] args)
        {
            //basic crappy non-production try/catch block. In real life, don't ever catch general exception e.
            try
            {
                Console.WriteLine("Begin outlier detection using K-Means clustering");
                Console.WriteLine("Loading investor data into memory");

                #region Investor Setup  //sets up dummy data
                string[] investorAttributes =
                    new string[] { "Net Worth", "Number of Investments" };

                double[][] dataFrame = new double[20][];    //in the real world, this will come from a file or DB

                dataFrame[0] = new double[] { 7500000.00, 4.0 };
                dataFrame[1] = new double[] { 21000000.00, 9.0 };
                dataFrame[2] = new double[] { 9100000.00, 6.0 };
                dataFrame[3] = new double[] { 65000000.00, 21.0 };
                dataFrame[4] = new double[] { 185000000.00, 8.0 };
                dataFrame[5] = new double[] { 113000000.00, 18.0 };
                dataFrame[6] = new double[] { 185000000.00, 4.0 };
                dataFrame[7] = new double[] { 6000000.00, 2.0 };
                dataFrame[8] = new double[] { 11000000.00, 64.0 };
                dataFrame[9] = new double[] { 9000000.00, 3.0 };
                dataFrame[10] = new double[] { 115000000.00, 1.0 };
                dataFrame[11] = new double[] { 225000000.00, 55.0 };
                dataFrame[12] = new double[] { 4500000.00, 2.0 };
                dataFrame[13] = new double[] { 18500000.000, 11.0 };
                dataFrame[14] = new double[] { 32000000.00, 12.0 };
                dataFrame[15] = new double[] { 550000000.00, 85.0 };
                dataFrame[16] = new double[] { 410000000.00, 9.0 };
                dataFrame[17] = new double[] { 7500000.00, 5.0 };
                dataFrame[18] = new double[] { 17500000.00, 9.0 };
                dataFrame[19] = new double[] { 75000000.00, 28.0 };
                #endregion

                //show the user the data
                Console.WriteLine("Investor Data:\tNet Worth\tNumber of Investments");
                PrintDataFrame(dataFrame);

                #region Cluster Setup //setup clustering config - # attributes, # clusters, and # iterations
                int numClusters = 4;    //exploratory analysis is the best way to figure this out
                int numAttributes = investorAttributes.Length;
                int maxIterations = 100;
                #endregion

                Console.WriteLine("\nBegin clustering data with k = {0} and iterations <= {1}",
                    numClusters, maxIterations);

                int[] clustering = Cluster(dataFrame, numClusters, maxIterations, numAttributes);

                Console.WriteLine("Clustering completed...formatting for readability");
                ShowVector(clustering);
                ShowClustering(dataFrame, numClusters, clustering);

            }
            catch (Exception e)
            {
                Console.Write(e.StackTrace);
            }
            finally
            {
                Console.WriteLine("\nEnd demo\n");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// PrettyPrints the membership of each cluster
        /// </summary>
        /// <param name="dataFrame">the data that has been clustered</param>
        /// <param name="numClusters">the number of clusters...I could probably derive this, but fuck you</param>
        /// <param name="clustering">the current clustering of the investors</param>
        private static void ShowClustering(double[][] dataFrame, int numClusters, int[] clustering)
        {
            Console.WriteLine("--------Riiiiiiiiccccoooooooolllllaaaaaa!!!!!-------");  //funky dividing line
            for (int x = 0; x < numClusters; x++)   //display each cluster
            {
                for (int y = 0; y < dataFrame.Length; y++)  //display each investor within a cluster
                {
                    if (clustering[y] == x) //current invstor y belongs to current cluster x
                    {
                        Console.Write("[{0}]\t", y.ToString().PadLeft(2));    //the investor...we could have a name, but...well, don't make me swear at you again, OK?
                        for (int z = 0; z < dataFrame[y].Length; z++) //each attribute within the investor
                            Console.Write(dataFrame[y][z].ToString("F1").PadLeft(6)+ " ");
                        Console.WriteLine("");  //bastard way of newline-ing
                    }
                }
                Console.WriteLine("-------------------------------------------------");
            }

            //one more newline
            Console.WriteLine("");
        }

        /// <summary>
        /// Pretty-prints integer array
        /// </summary>
        /// <param name="vector">Current clustering of the investors</param>
        private static void ShowVector(int[] vector)
        {
            for (int i = 0; i < vector.Length; i++)
                Console.Write(vector[i] + " ");
            
            //couple of spaces
            Console.WriteLine("/n/n");

        }

        /// <summary>
        /// Prints out the passed matrix (in statistics we refer to vectors or dataframes) to the console
        /// so the user understands the data we're working with
        /// </summary>
        /// <param name="dataframe">the dataframe/matrix we're printing to the console</param>
        static void PrintDataFrame(double[][] dataframe)
        {
            for (int i = 0; i < dataframe.Length; i++)
            {
                //this basically is our indexer: 0, 1, 2...
                Console.Write("[{0}]\t", i.ToString().PadLeft(2));

                //this loops through all attributes j within index i (networth/numinvestments)
                for (int j = 0; j < dataframe[i].Length; j++)
                {
                    Console.Write("\t" + dataframe[i][j].ToString("F1") + "  ");
                }
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// This function loops through Euclidian space trying to find clusters based on distance from a centroid
        /// (K-Means clustering), until wither (A) it finds the minimum mean, or (b) it maxes the allowed iterations
        /// </summary>
        /// <param name="data">the data matrix to be clustered.</param>
        /// <param name="numClusters">the number of clusters to create</param>
        /// <param name="maxIterations">the number of iterations to cap</param>
        /// <param name="numAttributes">the number of attributes we're tracking</param>
        /// <returns></returns>
        static int[] Cluster(double[][] data, int numClusters, int maxIterations, int numAttributes)
        {
            bool isChanged = true;  //we use this to tell if we hit min mean prior to maxIterations
            int count = 0;          // while (count <= maxCount)

            //this sets each investor to a random cluster to start
            int[] clustering = InitializeCluster(data.Length, numClusters);

            //I keep track of the means and centroids separately because I think it's cleaner, but my buddy
            //Mr. Newhouse ain't a fan of this approach. Fuck you, Dave. Fuck you.
            double[][] means = Allocate(numClusters, numAttributes);
            double[][] centroids = Allocate(numClusters, numAttributes);

            //after allocating space for the arrays, update the means and the centroids
            UpdateMeans(data, clustering, means);
            UpdateCentroids(data, clustering, means, centroids);

            while (isChanged && count < maxIterations)
            {
                count++;
                isChanged = Assign(data, clustering, centroids);

                //update means and centroids with new clustering
                UpdateMeans(data, clustering, means);
                UpdateCentroids(data, clustering, means, centroids);
            }

            return clustering;
        }

        /// <summary>
        /// Assign's each row/tupple (investor) to the cluster with the closest centroid
        /// </summary>
        /// <param name="data">the data being clustered</param>
        /// <param name="clustering">the current clustering of the investors</param>
        /// <param name="centroids">the current cluster centroids</param>
        /// <returns>Returns true if the function moved an investor to a new cluster</returns>
        private static bool Assign(double[][] data, int[] clustering, double[][] centroids)
        {
            int numClusters = centroids.Length; //there are as many clusters as there are centers of clusters
            bool wasChanged = false;    //nothing's changed yet...

            double[] distances = new double[numClusters];
            for (int i = 0; i < data.Length; i++)
            {
                for (int k = 0; k < numClusters; k++)
                    distances[k] = Distance(data[i], centroids[k]);

                int newCluster = MinIndex(distances);

                //Assigned to different cluster?
                if (newCluster != clustering[i])
                {
                    wasChanged = true;
                    clustering[i] = newCluster;
                }
            }

            return wasChanged;
        }

        /// <summary>
        /// finds the index of the smallest value in distances
        /// </summary>
        /// <param name="distances">Array of the current disances to a cluster centroid</param>
        /// <returns>integer representing index of the nearest cluster</returns>
        private static int MinIndex(double[] distances)
        {
            int index = 0;
            double shortestDistance = distances[0];

            for (int i = 0; i < distances.Length; i++)
            {
                if (distances[i] < shortestDistance)
                {
                    shortestDistance = distances[i];
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// This function updates centroids by handing off to helper functions to compute each
        /// individual cluster's centroid
        /// </summary>
        /// <param name="data">the data being clustered</param>
        /// <param name="clustering">the current clustering of the investors</param>
        /// <param name="means">the current means array</param>
        /// <param name="centroids">the current centroids array</param>
        private static void UpdateCentroids(double[][] data, int[] clustering, double[][] means, double[][] centroids)
        {
            //update all centroids by calling helper that updates individual centroids. 
            for (int k = 0; k < centroids.Length; k++)
            {
                double[] centroid = ComputeCentroid(data, clustering, k, means);
                centroids[k] = centroid;
            }
        }

        /// <summary>
        /// This computes the space space closest to each clustered mean
        /// </summary>
        /// <param name="data">the data being clustered</param>
        /// <param name="clustering">the current clustering of the investors</param>
        /// <param name="currentCluster">the current cluster being worked with</param>
        /// <param name="means">the means array of the clustered data</param>
        /// <returns>a double array representing the new centroid of a cluster</returns>
        private static double[] ComputeCentroid(double[][] data, int[] clustering, int currentCluster, double[][] means)
        {
            int numAttributes = means[0].Length;
            double[] centroid = new double[numAttributes];
            double minDistance = double.MaxValue;

            for (int i = 0; i < data.Length; i++)   //walk through each investor
            {
                int c = clustering[i];  //if the current investor isn't in our current cluster, continue
                if (c != currentCluster) continue;

                double currentDistance = Distance(data[i], means[currentCluster]);

                if (currentDistance < minDistance)
                {
                    //then we have found a better centroid!
                    minDistance = currentDistance;

                    for (int j = 0; j < centroid.Length; j++)
                        centroid[j] = data[i][j];
                }
            }

            return centroid;
        }

        /// <summary>
        /// This function computes the Euclidian distance between a datapoint and the centroid of a cluster
        /// </summary>
        /// <param name="dataRow">the current row, representing an investor and its attributes</param>
        /// <param name="clusterMean">the current mean of the distance between all points in this cluster and the 
        /// cluster's centroid</param>
        /// <returns>a double which represents the square root of the sum of squared differences. I can't explain
        /// what that means to you here, so go Google shit, laddie.</returns>
        private static double Distance(double[] dataRow, double[] clusterMean)
        {
            double sumSquaredDif = 0.0;
            for (int i = 0; i < dataRow.Length; i++)
                sumSquaredDif += Math.Pow((dataRow[i] - clusterMean[i]), 2);

            return Math.Sqrt(sumSquaredDif);
        }

        /// <summary>
        /// So this isn't perfect; it assumes means[][] exists, and that everything actually is in a cluster
        /// If it is, it then calculates the mean of the sum of the counts in a cluster for each attribute
        /// </summary>
        /// <param name="data">the data matrix we're working with</param>
        /// <param name="clustering">the current clustered state of the investors</param>
        /// <param name="means">the mean of the sum of the investors' attributes</param>
        private static void UpdateMeans(double[][] data, int[] clustering, double[][] means)
        {
            int numClusters = means.Length;

            //put in zeroes for each mean
            for (int j = 0; j < means.Length; j++)
                for (int k = 0; k < means[j].Length; k++)
                    means[j][k] = 0.0;

            //array to hold the count of investors in each cluster
            int[] clusterCounts = new int[numClusters];

            //loop through each investor and add the sum of each attribute in the cluster
            for (int i = 0; i < data.Length; i++)
            {
                int currentCluster = clustering[i];
                clusterCounts[currentCluster]++;

                for (int j = 0; j < data[i].Length; j++)
                    means[currentCluster][j] += data[i][j];
            }

            //now that we have the sum, divide by the count ina  cluster to get the mean
            for (int j = 0; j < means.Length; j++)
                for (int k = 0; k < means[j].Length; k++)
                    means[j][k] /= clusterCounts[k];

            return;
        }

        /// <summary>
        /// This function helps allocate means[][] and centroids[][] arrays
        /// </summary>
        /// <param name="numClusters">The numbersof clusters we're creating</param>
        /// <param name="numAttributes">the number of attributes in a cluster</param>
        /// <returns></returns>
        private static double[][] Allocate(int numClusters, int numAttributes)
        {
            double[][] result = new double[numClusters][];

            for (int k = 0; k < numClusters; k++)
                result[k] = new double[numAttributes];

            return result;
        }

        /// <summary>
        /// This function assigns each tuple (in this case, an investor) into a random cluster
        /// </summary>
        /// <param name="numInvestors"></param>
        /// <param name="numClusters"></param>
        /// <returns></returns>
        private static int[] InitializeCluster(int numInvestors, int numClusters)
        {
            int randomSeed = (int)DateTime.Now.ToFileTime();
            Random randomInt = new Random(randomSeed);
            int[] cluster = new int[numInvestors];

            //assign first investors to clusters 0...k-1
            for (int i = 0; i < numClusters; i++)
                cluster[i] = i;

            //now assign the rest randomly
            for (int i = numClusters; i < cluster.Length; i++)
                cluster[i] = randomInt.Next(0, numClusters);

            return cluster;
        }
    }
}
