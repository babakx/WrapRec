<img src='http://babakx.github.io/WrapRec/logo.png' height='100' />

WrapRec is an open-source project, developed with C# which aims to simplify the evaluation of Recommender System algorithms. WrapRec is a configuration-based tool. All desing choices and parameters should be defined in a configuration file.

Check [WrapRec website](http://babakx.github.io/WrapRec) for more information about WrapRec.


##Features

* __Easy to use__
To perform experiments with WrapWrec all you need to do is to write your settings in a configuration file.

* __Wrap Multiple Algorithms, Evaluate on Single Framework__
WrapRec is designed to wrap multiple algorithms (from different toolkits) and evaluate your model under a single evaluation framework.

* __Multiple experiments in one Run__
In WrapRec parameters (that you define in the config file) can have multiple values. WrapRec detects those values and run evaluation experiments as many times as the cartesian product of all those parameters.

* __Context-aware Recommendation__
WrapRec contain components that makes it easy to perform context-aware recommendations.

* __Multiple Evaluation Method__
With WrapRec you can run multiple evaluation methods on a single algorithm and compare the results.

* __Easily Extendible__
Without requiring to modify the WrapRec source code, you can define your own extension and plug it into the framework

## Using WrapRec
WrapRec is a configuration-based tool. All the settings and design choices should be defined in a configuration file.
After configuring the configuration file, WrapRec can be simply used by the following command:

####Windowds
```
wraprec.exe [config.xml]
```

####Linux and Mac
```
mono wraprec.exe [config.xml]
```

For more details, see [Get Started with WrapRec](http://babakx.github.io/WrapRec/GetStarted.html).

### License
WrapRec is under the [MIT License](https://opensource.org/licenses/MIT).
