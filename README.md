WrapRec
=========================

A toolkit to implement recommender system algorithms

WrapRec is a toolkit, implemented in C#, for recommender system algorithms.

Check the [Wiki page](https://github.com/babakx/WrapRec/wiki) for more documenations about WrapRec.


##Features

* __Easy to use__
The most important feature of this toolbox is to be able to run a recommender algorithm in a few clear steps.

* __Common interface for reading datasets with different format__
The toolbox is customizable to read any data format. 

* __Cross-Domain recommendation__
The toolbox provide interfaces for Cross-Domain recommendation in a object-oriented manner.

* __Pipeline-based evaluation__
Evaluation of different algorithms can be done in a pipeline-based-pattern mechanism. This allows to easily use different evaluators for an experiment.

* __Context-aware recommendation__
The toolbox is highly customizable to handle context-aware recommendation algorithms.

* __Relational data interface__
The toolbox provides an additional layer of data management which allows the consumers of data to not only read relational data, but also enables users to access flat data files with a relational data-access manner. This feature benefits from the advatages of the [Micorsoft Entity Framework](http://msdn.microsoft.com/en-us/data/ef.aspx) as well as the .Net Language Intergrated Query (LINQ) to do data access in a felixible way.

* __Exetensions of existing libraries__
The object model of WrapRec allows this toolbox to easily integrate existing toolboxes into the model and benefits from the existing code. Currently this toolbox has wrappers around [MyMediaLite](http://www.mymedialite.net), a recommendation framework wrriten in C#, and [LibFM](http://www.libfm.org), a library for factorization machines written in C++.

## Usage Samples

#### Simple train and test scenario
With a few lines of code you can run a full train/test scenario. This can be done in 3 steps: 1. Define a dataset 2. Define a recommender algorithm 3. Evaluate.

Below is a sample code, which loads Movielens data, splits it into 70% training set and 30% testset and evaluates it based on the BiasedMatricFactorization algorithm. The evaluation metrics are RMSE and MAE.

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
WrapRec is capable of easily incorporating custom data formats. In the above example, you can replace class ```MovieLensReader``` with a custom dataset reader. Any class which implements the interface ```IDatasetReader<T>``` can be incoporated into the mode.
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
The framework provides a configurable evaluation mechanism through pipeline pattern. If you want to have your own custom evaluator, all you need is to implement the interface ```IEvaluator```. This method already provides a context object which includes the results of the tested samples.

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
With a similar logic, custom recommendation algorithms can be implemented by implementing either interface ```IPredictor<T>``` for rating prediction tasks, or interface ```IItemRecommender``` for item recommendation tasks. Here we already wrapped MyMediaLite rating algorithms into the toolbox. Following you can see a sample of a custom recommender algorithm.


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
The project can be easily installed by downloading the package from [NuGet] (http://www.nuget.org/packages/WrapRec.dll/) gallery.
This project is quite new. The documentation is continuously updated. Please contact Babak Loni, (babak dot loni At gmail dot com) for questions about this toolbox.

##Acknowledgment
The implementation of this project is supported by the European 7th Framework Program projects under grant agreements no. 610594 ([CrowdRec](http://www.crowdrec.eu/)).
