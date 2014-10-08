using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRecDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            string usage = @"WrapRec Demo 1.0. This demo provide a limited funtionality of the WrapRec. To use the full funcionaly of WrapRec use the WrapRec .NET APIs.

 usage WrapRecDemo [options]

 options:

    --problem=rp|ir                       

 specifies the type of problem (rp: rating prediction from explicit ratings, ir: item recommendation from implicit data)

    --dataset=FILE                        

 specifies the dataset file

    --data-format=csv|crowdrec|libfm      

 format of dataset file(s) csv: csv file with format (userId,itemId,rating), crowdrec: crowdrec datamodel format, libfm: libfm format (rating [featureId:value])
    
    --testportion=VAL                     

 devide dataset into train/test portion based on value (0 < VAL < 1)
    
    --trainfile=FILE                      

 path to train file (this option and --dataset option can not be used simultaneously)

    --testfile=FILE                       

 path to test file (this option and --dataset option can not be used simultaneously)
    
    --entitiesfile=FILE                   

 path to entities file expected by crowdrec data format (this option and --dataset, --trainfile and --testfile options can not be used simultaneously)
    
    --relationsfile=FILE                  

 path to relations file expected by crowdrec data format (this option and --dataset, --trainfile and --testfile options can not be used simultaneously)

    --rp-algorithm=ALG                    

 specifies rating prediction algorithm. ALG should be one of the followings, mf: general Matrix Factorization, bmf: Biased Matrix Factorization, fm: Factorization Machines

    --ir-algorithm=ALG                    

 specifies item recommendation algorithm. ALG should be one of the followings, bpr: Bayesian Personalized Ranking, mp: Most Popular 

    --rp-eval=[EVALS]|all                 

 specifies rating prediction evaluation metric, [EVALS] is a comma separated evaluation metrics and can be from {mae,rmse}, all: use all available evaluation metrics for rating prediction (default: all)

    --ir-eval=[EVALS]|all                 

 specifies item recommendation evaluation metric, [EVALS] is a comma separated evaluation metrics and can be from {ndcg,p@n,r@n,map} (n is an arithmatic number), all: use all available evaluation metrics for rating prediction (n = 10) (default: all)

    --cross-domain=FILE                   

 cross-domain recommendation using additional auxiliary data, FILE: path to auxiliary dataset with format specified by parameter --data-format
";
            try
            {
                if (args.Length == 0)
                    Console.Write(usage);
                else
                    new Setup(args);
            }
            catch (WrapRec.WrapRecException ex)
            {
                Console.WriteLine("\nWrapRec Error: \n");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nGeneral Error: \n");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
