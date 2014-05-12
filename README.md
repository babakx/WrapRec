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
 
## Usage Sample

```javascript
// step 1: dataset            
var dataset = new Dataset<MovieLensItemRating>(new MovieLensReader("/ratings.dat"));

// step 2: recommender
var recommender = new MediaLiteRatingPredictor(new BiasedMatrixFactorization());

// step3: evaluation
var ep = new EvaluationPipeline<ItemRating>(new EvalutationContext<ItemRating>(recommender, dataset));
ep.Evaluators.Add(new RMSE());
ep.Evaluators.Add(new MAE());

ep.Run();
```
