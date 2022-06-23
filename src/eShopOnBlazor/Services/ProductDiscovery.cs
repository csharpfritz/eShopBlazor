using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace eShopOnBlazor.Services;

public class ProductDiscovery
{
	
	internal const string MODEL_PATH = "Setup/productModel.zip";
	MLContext _Context = new MLContext();
	ITransformer _Model;
	private PredictionEngine<ProductPurchase, ProductPrediction> _PredictionEngine;
	private readonly PredictionEnginePool<ProductPurchase, ProductPrediction> _PredictionEnginePool;

	public ProductDiscovery() { }

	public ProductDiscovery(PredictionEnginePool<ProductPurchase, ProductPrediction> predictionEnginePool)
	{

		_PredictionEnginePool = predictionEnginePool;

	}

	public static IServiceCollection AddProductRecommendations(IServiceCollection services)
	{

		services.AddPredictionEnginePool<ProductPurchase, ProductPrediction>()
			.FromFile(MODEL_PATH, true);
		_ = services.AddTransient(s => new ProductDiscovery(s.GetRequiredService<PredictionEnginePool<ProductPurchase, ProductPrediction>>()));

		return services;

	}

	/// <summary>
	/// Build a recommendation model based on the Follower data and save it to the stream provided
	/// </summary>
	/// <param name="followersAnalysis">Follower data to analyze</param>
	/// <param name="destinationStream">Stream to save the data to</param>
	/// <returns>Model appropriate for predicting channel interest</returns>
	public ITransformer BuildModel(
		IEnumerable<ProductPurchase> followersAnalysis,
		Stream destinationStream
	)
	{

		var trainingDataView = _Context.Data.LoadFromEnumerable(followersAnalysis);

		var estimator = _Context.Transforms.Conversion.MapValueToKey(outputColumnName: "productIdEncoded", inputColumnName: nameof(ProductPurchase.ProductId))
			.Append(_Context.Transforms.Conversion.MapValueToKey(outputColumnName: "otherProductIdEncoded", inputColumnName: nameof(ProductPurchase.OtherProductId)));

		var options = new MatrixFactorizationTrainer.Options
		{
			MatrixColumnIndexColumnName = "productIdEncoded",
			MatrixRowIndexColumnName = "otherProductIdEncoded",
			LabelColumnName = "Purchased",
			LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass,
			Alpha = 0.01,
			Lambda = 0.025,
			NumberOfIterations = 3000,
			C = 0.00001,
			Quiet = true
		};

		var trainerEstimator = estimator.Append(_Context.Recommendation().Trainers.MatrixFactorization(options));

		Console.WriteLine("=============== Training the model ===============");
		_Model = trainerEstimator.Fit(trainingDataView);

		Console.WriteLine("=============== Saving the model to a file ===============");

		//var modelPath = Path.Combine(Environment.CurrentDirectory, "ChannelRecommendationModel.zip");
		_Context.Model.Save(_Model, trainingDataView.Schema, destinationStream);

		return _Model;

	}

	public void LoadModel(Stream sourceStream)
	{
		_Model = _Context.Model.Load(sourceStream, out var schema);
		_PredictionEngine = _Context.Model.CreatePredictionEngine<ProductPurchase, ProductPrediction>(_Model);
	}

	public void LoadModel(string filePath)
	{
		_Model = _Context.Model.Load(filePath, out var schema);
		_PredictionEngine = _Context.Model.CreatePredictionEngine<ProductPurchase, ProductPrediction>(_Model);
	}

	public ProductPrediction Predict(string productId, string otherProductId)
	{


		var testInput = new ProductPurchase()
		{
			ProductId = productId,
			OtherProductId = otherProductId,
		};

		return _PredictionEngine?.Predict(testInput) ?? _PredictionEnginePool.Predict(testInput);

	}

	public IEnumerable<ProductRecommendation> SuggestProducts(string productId, IEnumerable<string> candidateProducts, int maxCount)
	{

		var outList = new List<ProductRecommendation>();

		foreach (var candidate in candidateProducts)
		{

			var input = new ProductPurchase()
			{
				ProductId = productId,
				OtherProductId = candidate
			};

			var score = _PredictionEngine?.Predict(input) ?? _PredictionEnginePool.Predict(input);
			if (float.IsNaN(score.Score)) continue;
			try
			{
				outList.Add(new ProductRecommendation
				{
					ProductId = candidate,
					Score = (decimal)Math.Round(score.Score, 5)
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(score.Score);
			}

		}

		return outList.OrderByDescending(c => c.Score).Take(maxCount).ToArray();

	}

}

public class ProductPrediction
{

	public float Score;

}

public class ProductPurchase
{

	public string ProductId;

	public string OtherProductId;

	public Single Purchased = 0;


}

public class ProductRecommendation
{

	public string ProductId;

	public decimal Score;

}
