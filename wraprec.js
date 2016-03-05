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
'    <model id="bprmf" class="WrapRec.Models.MmlRecommender">',
'      <parameters ml-class="MyMediaLite.dll:MyMediaLite.ItemRecommendation.BPRMF" NumFactors="10,20" NumIter="30,50" />',
'    </model>',
'    <model id="libfm" class="WrapRec.Models.LibFmWrapper">',
'      <parameters libFmPath="libfm.net.exe" task="r" dim="1-1-8" method="sgd,mcmc" iter="30,50" learn_rate="0.01" />',
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
'    <split id="ml100k-cv" type="crossValidation" dataContainer="ml100k" numFolds="5" />',
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
'    <experiment id="ml100k" models="bprmf,libfm" splits="ml100k-dynamic,ml100k-cv" evalContext="eval" />',
'  </experiments>',
'</wraprec>'].join('\n');


$('#xml1').text(xml1);



