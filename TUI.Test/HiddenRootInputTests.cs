// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

/// <summary>
/// A hidden element takes no input, whether it is a leaf or a container, and a hidden root is not
/// handed input by the run loop (ktsu-dev/TUI#266, after ktsu-dev/TUI#180 covered containers).
/// </summary>
[TestClass]
public sealed class HiddenRootInputTests
{
	/// <summary>
	/// How long a run is given to end before it is declared stuck.
	/// </summary>
	private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Gets or sets the test context MSTest injects.
	/// </summary>
	public TestContext TestContext { get; set; } = null!;

	/// <summary>
	/// A hidden leaf element does not run its input handler and reports the key unhandled.
	/// </summary>
	[TestMethod]
	public void HiddenLeafElementDoesNotHandleInput()
	{
		KeyCountingElement element = new() { IsVisible = false };

		bool handled = element.HandleInput(InputResult.FromKey(ConsoleKey.A));

		Assert.IsFalse(handled);
		Assert.AreEqual(0, element.HandledCount);
	}

	/// <summary>
	/// A visible leaf element still handles input.
	/// </summary>
	[TestMethod]
	public void VisibleLeafElementHandlesInput()
	{
		KeyCountingElement element = new();

		bool handled = element.HandleInput(InputResult.FromKey(ConsoleKey.A));

		Assert.IsTrue(handled);
		Assert.AreEqual(1, element.HandledCount);
	}

	/// <summary>
	/// A hidden custom root derived from <see cref="UIElementBase"/> sees no keys during a run.
	/// </summary>
	[TestMethod]
	public async Task HiddenLeafRootSeesNoKeysDuringARun()
	{
		KeyCountingElement root = new() { IsVisible = false };

		ScriptedConsoleProvider provider = await RunWithRootAsync(root).ConfigureAwait(false);

		Assert.AreEqual(0, root.HandledCount, "A hidden root should not handle the keys read while it is hidden");
		Assert.AreEqual(3, provider.ReadCount);
	}

	/// <summary>
	/// A hidden root that implements <see cref="IUIElement"/> directly is not handed input either.
	/// </summary>
	[TestMethod]
	public async Task HiddenRootImplementingTheInterfaceIsNotHandedInput()
	{
		Mock<IUIElement> root = new();
		root.Setup(r => r.IsVisible).Returns(false);
		root.Setup(r => r.HandleInput(It.IsAny<InputResult>())).Returns(true);

		await RunWithRootAsync(root.Object).ConfigureAwait(false);

		root.Verify(r => r.HandleInput(It.IsAny<InputResult>()), Times.Never);
	}

	/// <summary>
	/// A visible root still gets every key read before the exit key.
	/// </summary>
	[TestMethod]
	public async Task VisibleRootSeesEveryKeyDuringARun()
	{
		KeyCountingElement root = new();

		await RunWithRootAsync(root).ConfigureAwait(false);

		Assert.AreEqual(2, root.HandledCount);
	}

	private async Task<ScriptedConsoleProvider> RunWithRootAsync(IUIElement root)
	{
		ScriptedConsoleProvider provider = new(InputResult.FromKey(ConsoleKey.A), InputResult.FromKey(ConsoleKey.B));
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };
		app.Setup(root);

		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "The run should end on the exit key that follows the scripted keys");
		await run.ConfigureAwait(false);
		return provider;
	}

	/// <summary>
	/// A leaf element that counts the keys it handles and claims each one.
	/// </summary>
	private sealed class KeyCountingElement : UIElementBase
	{
		public int HandledCount { get; private set; }

		protected override void OnRender(IConsoleProvider provider) { }

		protected override bool OnHandleInput(InputResult input)
		{
			HandledCount++;
			return true;
		}
	}

	/// <summary>
	/// A provider that returns the given inputs in order, then exit keys.
	/// </summary>
	private sealed class ScriptedConsoleProvider(params InputResult[] inputs) : IConsoleProvider
	{
		private int readCount;

		public int ReadCount => Volatile.Read(ref readCount);

		public Dimensions Dimensions => new(80, 24);

		public void Clear() { }

		public void Render(IUIElement element, Position position) => element?.Render(this);

		public void WriteAt(string text, Position position, TextStyle? style = null) { }

		public Task<InputResult> ReadInputAsync()
		{
			int count = Interlocked.Increment(ref readCount);
			return Task.FromResult(count <= inputs.Length ? inputs[count - 1] : InputResult.Exit());
		}

		public void SetCursorVisibility(bool visible) { }

		public void SetCursorPosition(Position position) { }
	}
}
