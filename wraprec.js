$('#sidebar').affix({
	offset: {
		top: 245
	}
});

var $body = $(document.body);
var navHeight = $('.navbar').outerHeight(true) + 10;

$body.scrollspy({
	target: '#leftCol',
	offset: navHeight
});

$('#header').load("http://babakx.github.io/WrapRec/header.html");

var xml1 = ['<wraprec>',
'  <models>',
'    <model id="mf" class="WrapRec.Models.MmlRecommender">',
'      <parameters ml-class="MyMediaLite.dll:MyMediaLite.RatingPrediction.MatrixFactorization" NumFactors="10,20" NumIter="30,50" />',
'    </model>',
'    <model id="bmf" class="WrapRec.Models.MmlRecommender">',
'      <parameters ml-class="MyMediaLite.dll:MyMediaLite.RatingPrediction.BiasedMatrixFactorization" NumFactors="10,20" NumIter="30,50" />',
'    </model>',
 '  </models>',
'  <data>',
'    <dataReaders>',
'      <reader id="ml100k-all" path="ml-100k/u.data" sliceType="other" dataType="ratings"',
'        class="WrapRec.IO.CsvReader" hasHeader="false" delimiter="\\t" />',
'    </dataReaders>',
'    <dataContainers>',
'      <dataContainer id="ml100k" dataReaders="ml100k-all" />',
'    </dataContainers>',
'  </data>',
'  <splits>',
'    <split id="ml100k-dynamic" type="dynamic" dataContainer="ml100k" trainRatios="0.75" />',
'  </splits>',
'  <evalContexts>',
'    <evalContext id="eval">',
'      <evaluator class="WrapRec.Evaluation.RMSE" />',
'      <evaluator class="WrapRec.Evaluation.MAE" />',
'      <evaluator class="WrapRec.Evaluation.RankingEvaluators" candidateItemsMode="training" numCandidates="1000" cutOffs="5,10,20" />',
'    </evalContext>',
'  </evalContexts>',
'  <experiments run="ml100k" resultsFolder="results" separator="\\t" verbosity="info">',
'    <experiment id="ml100k" models="mf,bmf" splits="ml100k-dynamic" evalContext="eval" />',
'  </experiments>',
'</wraprec>'].join('\n');


var confFomat = ['<wraprec>',
'  <models>',
'    <model id="model1" model1-parameters />',
'    <model id="model2" model2-parameters />',
'    <model id="model-k" model-k-parameters />',
'  </models>',
'  <data>',
'    <dataReaders>',
'      <reader id="reader1" reader1-parameters />',
'      <reader id="reader2" reader2-parameters />',
'    </dataReaders>',
'    <dataContainers>',
'      <dataContainer id="container1" container1-parameters />',
'      <dataContainer id="container2" container1-parameters />',
'    </dataContainers>',
'  </data>',
'  <splits>',
'    <split id="split1" split1-parameters />',
'    <split id="split2" split2-parameters />',
'  </splits>',
'  <evalContexts>',
'    <evalContext id="evalContext1">',
'      <evaluator parameter />',
'      <evaluator parameter />',
'      <evaluator parameter />',
'    </evalContext>',
'  </evalContexts>',
'  <experiments run="exp1,exp2" resultsFolder="results" verbosity="info">',
'    <experiment id="exp1" models="model1,model2" splits="split1" evalContext="evalContext1" />',
'    <experiment id="exp2" models="model-k" splits="split2" evalContext="evalContext1" />',
'  </experiments>',
'</wraprec>'].join('\n');


$('#xml1').text(xml1);
$('#confFomat').text(confFomat);



