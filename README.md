WrapRec
=========================

A toolkit to implement recommender system algorithms

WrapRec is a toolkit, implemented with C#, for recommender system algorithms.


##Features

* __Easy to use__
The most important feature of this toolbox is to be able to run a recommender algorithm in a few clear step.

* __Common interface for reading dataset with different format__
The toolbox is highly customizable to read any data format. 

* __Cross-Domain recommendation__
The toolbox provide interfaces for Cross-Domain recommendation in a object-oriented manner.

* __Pipeline-base evaluation__
Evaluation of different algorithms can be done by a pipeline-based-pattern mechanism. This allow the easily use different evaluators for an experiment.

* __Context-aware recommendation__
The toolox is highly customizable to handle context-aware recommendation algorithms.

* __Relational data interface__
This toolbox provide additional layer of data management which allows the consumers of data to not only read relational data, but also enables users to access flat data files with a relational data-access manner. This feature allows to benefits from the advatages of [Micorsoft Entity Framework] (http://msdn.microsoft.com/en-us/data/ef.aspx) as well as .Net Language Intergrated Query (LINQ) to do data access in a felixible way.

* __Exetensions on existing libraries__
The object model of RF2 allows this toolbox to easily integare exisign toolboxes into the model and benefits from the exisiting codes. Currently this toolbox have wrappers around [MyMediaLite] (http://www.mymedialite.net), a recommendation framework wrriten in C#, and [LibFM] (http://www.libfm.org), a library for factorization machines written in C++.

## Usage Samples

#### Simple train and test scenario
With a few lines of code you can alreay run a full train/test scenario. This can be done in 3 steps: 1. Define a dataset 2. Define a recommender algorithm 3. Evaluate

Following is a sample code, which loads MovieLens data, split it into 70% trainset and 30% testset and evaluate it based on BiasedMatricFactorization algorithm. The evaluation metrics are RMSE and MAE.

```javascript
// step 1: dataset            
var dataset = new Dataset<MovieLensItemRating>(new MovieLensReader("/ratings.dat"), 0.7);

// step 2: recommender
var recommender = new MediaLiteRatingPredictor(new BiasedMatrixFactorization());

// step3: evaluation
var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
ep.Evaluators.Add(new RMSE());
ep.Evaluators.Add(new MAE());

ep.Run();
```

#### Custom Data Reader
RF2 is capable of easily incorporating custom data format into it. In the above example, you can replace class ```MovieLensReader``` with a custom dataset reader. Any class which implements the interface ```IDatasetReader<T>``` can be incoporated into the mode.
Here is an example of a custom dataset reader.

```javascript
public class MyCustomDataReader : IDatasetReader<ItemRating>
{
    string _datasetPath;

    public MyCustomDataReader(string datasetPath)
    {
        _datasetPath = datasetPath;
    }
    
    public IEnumerable<ItemRating> ReadSamples()
    {
        // logic to read data is here
    }
}
```

#### Custom Evaluator
The framework provide a configurable evaluation mechanism through pipeline pattern. If you want to have your own cutom evaluator, all you need is to implement the interface ```IEavluator```. This method already provide you a context onbject which include the results of the tested samples.

```javascript
public class CustomEvaluator : IEvaluator<ItemRating>
{
    public void Evaluate(EvalutationContext<ItemRating> context)
    {
        // make sure that the test samples are predicted
        context.RunDefaultTrainAndTest();
        
        // here you can access to tested samples
        var testset = context.Dataset.TestSamples;

        // here is the evaluation logic
        double metric = LogicToCalculateMetic(testset);
        
        // here you update the context with calculated metric for posisble re-use by other evaluators
        context["CustomEvaluator"] = metric
    }
}
```

### Custom Recommender Algorithm
With a similar logic, custom recommenation algorithm can be implemented by implementing either interface ```IPredictor<T>``` for rating prediction tasks, or interface ```IItemRecommender``` for item recommendation tasks. Here we already wrrapped MyMediaLite rating algorithms into toolbox. Following you can see a sample of a custom recommender algorithm.


```javascript
public class CustomRatingPredictor : IPredictor<ItemRating>
{
    bool _isTrained;
    Model _trainedModel;

    public void Train(IEnumerable<ItemRating> trainSet)
    {
        // logic for training is here
        // ...
        
        // _trainedModel is build

        // if training was successfull
        _isTrained = true;
    }

    public void Predict(ItemRating sample)
    {
        // update the sample with the predicted rating
        sample.PredictedRating = _trainedModel.Predict(sample);
    }
    
    public bool IsTrained
    {
        get
        {
            return _isTrained;
        }
    }
}
```

## Usage and Documentations
Currently this toolbox can be use either directly by downloading the source code or can be imported as a dll to your project. The dll can be downloaded [here] (https://github.com/babakx/Recommendation-Framework2/blob/master/RecommendationFramework2.dll). 
This project is quite new. The documenation will be completed in early future. Please contact Babak Loni, (babak dot loni At gmail dot com) for questions about this toolbox.
