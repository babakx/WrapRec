Recommendation-Framework2
=========================

A framework to implement recommender system algorithms

Recommendation Framework2 is a toolbox, implemented with C#, for recommender system algorithms.


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
The object model of RF2 allows this toolbox to easily integare exisign toolboxes into the model and benefits from the exisiting codes. Currently this toolbox have wrappers around [MyMediaLite] (www.mymedialite.net), a recommendation framework wrriten in C#, and [LibFM] (www.libfm.org), a library for factorization machines written in C++.

## Usage Sample

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
