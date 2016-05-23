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

$('#btnDownloadLatestWin').click(function () {
	$.ajax({
		type: 'GET',
		url: 'https://api.github.com/repos/babakx/WrapRec/releases/latest',
		dataType: 'json',
		success: function (data) {
			window.location.href = data.assets[1].browser_download_url;
		}
	});
});

$('#btnDownloadLatestMono').click(function () {
    $.ajax({
        type: 'GET',
        url: 'https://api.github.com/repos/babakx/WrapRec/releases/latest',
        dataType: 'json',
        success: function (data) {
            window.location.href = data.assets[0].browser_download_url;
        }
    });
});


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

var expEl = ['<experiment id="[experimentId]" models="[list of models comma seperated]"',
    '   splits="[list of splits comma separated]" evalContext="[evalContextId]" />'].join('\n');

var modelEl = ['<model id="mf" class="WrapRec.Models.MmlRecommender">',
'   <parameters ml-class="MyMediaLite.dll:MyMediaLite.RatingPrediction.MatrixFactorization"',
'       NumFactors="10,20" NumIter="30,50" />',
'</model>'
].join('\n');

var splitEl = ['<split id="[splitId]" type="[static|dynamic|cv|custom]" dataContainer="[containerId]"', 
    '   trainRatios="[a value between 0 and 1]" numFolds="[num folds for cv type]"',
    '   class="[type name for custom split]" other-parameters />'].join('\n');

var containerEl = '<dataContainer id="[containerId]" dataReaders="[comma separated list of reader ids]" other-parameters />';

var readerEl = ['<reader id="[readerId]" path="[path to the dataset]" sliceType="[train|test|other]"',
    '   dataType="[posFeedback|ratings]" class="[reader class]" other-parameters />'].join('\n');

var evalEl = ['<evalContext id="[evalContextId]">',
'   <evaluator class="[type of evaluator class]" other-parameters />',
'</evalContext>',
].join('\n');

var evalElExample = ['<evalContext id="eval">',
'   <evaluator class="WrapRec.Evaluation.RMSE" />',
'   <evaluator class="WrapRec.Evaluation.MAE" />',
'   <evaluator class="WrapRec.Evaluation.RankingEvaluators" candidateItemsMode="training"', 
'       numCandidates="1000" cutOffs="5,10,20" />',
'</evalContext>', ].join('\n');


$('#xml1').text(xml1);
$('#confFomat').text(confFomat);
$('#expEl').text(expEl);
$('#modelEl').text(modelEl);
$('#splitEl').text(splitEl);
$('#containerEl').text(containerEl);
$('#readerEl').text(readerEl);
$('#evalEl').text(evalEl);
$('#evalElExample').text(evalElExample);





