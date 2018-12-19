﻿using System;
using System.Collections.Generic;
using System.Linq;
using Duality.Resources;
using NUnit.Framework;

namespace Duality.Tests.Resources
{
	public class RectAtlasTest
	{
		[Test] public void Constructor()
		{
			RectAtlas atlas1 = new RectAtlas();
			Assert.AreEqual(0, atlas1.Count);
			RectAtlas atlas2 = new RectAtlas(10);
			Assert.AreEqual(0, atlas2.Count);

			Rect[] rects = { new Rect(1, 1, 1, 1), new Rect(2, 2, 2, 2) };
			RectAtlas atlas3 = new RectAtlas(rects);
			CollectionAssert.AreEquivalent(rects, atlas3);
		}

		[Test] public void ConstructorsThrowException()
		{
			Assert.Throws<ArgumentNullException>(() => new RectAtlas((IEnumerable<Rect>)null));
			Assert.Throws<ArgumentNullException>(() => new RectAtlas((RectAtlas)null));
		}

		[Test] public void Basics()
		{
			RectAtlas atlas = new RectAtlas();

			atlas.Add(new Rect(1, 1, 1, 1));
			Assert.IsTrue(atlas.Contains(new Rect(1, 1, 1, 1)));
			Assert.AreEqual(Vector2.Zero, atlas.GetPivot(0));
			Assert.AreEqual(null, atlas.GetTag(0));
			Assert.AreEqual(1, atlas.Count);
			Assert.AreEqual(0, atlas.IndexOf(new Rect(1, 1, 1, 1)));

			atlas.Add(new Rect(2, 2, 2, 2));
			Assert.IsTrue(atlas.Contains(new Rect(1, 1, 1, 1)));
			Assert.IsTrue(atlas.Contains(new Rect(2, 2, 2, 2)));
			Assert.AreEqual(2, atlas.Count);
			Assert.AreEqual(0, atlas.IndexOf(new Rect(1, 1, 1, 1)));
			Assert.AreEqual(1, atlas.IndexOf(new Rect(2, 2, 2, 2)));

			Assert.IsTrue(atlas.Remove(new Rect(1, 1, 1, 1)));
			Assert.IsFalse(atlas.Contains(new Rect(1, 1, 1, 1)));
			Assert.AreEqual(1, atlas.Count);
			Assert.AreEqual(0, atlas.IndexOf(new Rect(2, 2, 2, 2)));

			atlas.Clear();
			Assert.IsFalse(atlas.Contains(new Rect(2, 2, 2, 2)));
			Assert.AreEqual(0, atlas.Count);
			Assert.AreEqual(-1, atlas.IndexOf(new Rect(2, 2, 2, 2)));

			Assert.IsFalse(atlas.Remove(new Rect()));
		}

		[Test] public void Insert()
		{
			RectAtlas atlas = new RectAtlas();

			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.Insert(-1, new Rect()));
			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.Insert(1, new Rect()));

			atlas.Insert(0, new Rect());
			Assert.IsTrue(atlas.Contains(new Rect()));
			Assert.AreEqual(0, atlas.IndexOf(new Rect()));

			atlas.SetTag(0, "Tag");

			// Inserting at a previously tagged index. Make sure the tagged index gets updated
			// to the rects new index after inserting another rect.
			atlas.Insert(0, new Rect(1, 1, 1, 1));
			Assert.IsTrue(atlas.Contains(new Rect(1, 1, 1, 1)));
			Assert.AreEqual(0, atlas.IndexOf(new Rect(1, 1, 1, 1)));
			Assert.AreEqual(1, atlas.IndexOf(new Rect()));
			Assert.AreEqual(null, atlas.GetTag(0));
			Assert.AreEqual("Tag", atlas.GetTag(1));
		}

		[Test] public void RemoveAt()
		{
			RectAtlas atlas = new RectAtlas();

			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.RemoveAt(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.RemoveAt(1));

			atlas.Add(new Rect());
			atlas.RemoveAt(0);
			Assert.IsFalse(atlas.Contains(new Rect()));

			atlas.Add(new Rect());
			atlas.SetTag(0, "Tag0");
			atlas.Add(new Rect(1, 1, 1, 1));
			atlas.SetTag(1, "Tag1");

			atlas.RemoveAt(0);
			Assert.AreEqual("Tag1", atlas.GetTag(0));
			CollectionAssert.AreEquivalent(new[] { 0 }, atlas.GetTaggedIndices("Tag1"));
			CollectionAssert.IsEmpty(atlas.GetTaggedIndices("Tag0"));
		}

		[Test]
		public void GetSetPivot()
		{
			RectAtlas atlas = new RectAtlas();

			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.SetPivot(-1, Vector2.Zero));
			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.SetPivot(0, Vector2.Zero));
			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.GetPivot(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.GetPivot(0));

			atlas.Add(new Rect(0, 0, 0, 0));

			atlas.SetPivot(0, new Vector2(1, 1));
			Assert.AreEqual(new Vector2(1, 1), atlas.GetPivot(0));
			atlas.SetPivot(0, new Vector2(0, 0));
			Assert.AreEqual(new Vector2(0, 0), atlas.GetPivot(0));
		}

		[Test] public void TagAndUnTag()
		{
			RectAtlas atlas = new RectAtlas();

			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.SetTag(-1, null));
			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.SetTag(0, null));
			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.GetTag(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => atlas.GetTag(0));
			Assert.Throws<ArgumentNullException>(() => atlas.SetTags("Tag", null));

			atlas.SetTags("Non-ExistantTag", new int[0]);
			CollectionAssert.IsEmpty(atlas.GetTaggedIndices("Non-ExistantTag"));

			atlas.SetTags("OutOfRange", new[]{ 5 });
			CollectionAssert.IsEmpty(atlas.GetTaggedIndices("OutOfRange"));

			atlas.Add(new Rect());
			atlas.SetTags("Tag1", new[] { 0 });
			CollectionAssert.AreEquivalent(new[] { 0 }, atlas.GetTaggedIndices("Tag1"));
			CollectionAssert.AreEquivalent(new[] { new Rect() }, atlas.GetTaggedRects("Tag1"));

			atlas.Add(new Rect(1, 1, 1, 1));
			atlas.SetTags("Tag1", new[] { 1 });
			CollectionAssert.AreEquivalent(
				new[] { 0, 1 },
				atlas.GetTaggedIndices("Tag1"));
			CollectionAssert.AreEquivalent
				(new[] { new Rect(), new Rect(1, 1, 1, 1), },
				atlas.GetTaggedRects("Tag1"));

			atlas.SetTags("Tag2", new[] { 1 });
			CollectionAssert.AreEquivalent(new[] { 0 }, atlas.GetTaggedIndices("Tag1"));
			CollectionAssert.AreEquivalent(new[] { new Rect() }, atlas.GetTaggedRects("Tag1"));
			CollectionAssert.AreEquivalent(new[] { 1 }, atlas.GetTaggedIndices("Tag2"));
			CollectionAssert.AreEquivalent(new[] { new Rect(1, 1, 1, 1) }, atlas.GetTaggedRects("Tag2"));

			atlas.SetTags(null, new[] { 0 });
			Assert.AreEqual(1, atlas.GetTaggedIndices(null).Count());
			Assert.AreEqual(1, atlas.GetTaggedRects(null).Count());
			CollectionAssert.IsEmpty(atlas.GetTaggedIndices("Tag1"));
			CollectionAssert.IsEmpty(atlas.GetTaggedRects("Tag1"));
			CollectionAssert.AreEquivalent(new[] { 1 }, atlas.GetTaggedIndices("Tag2"));
			CollectionAssert.AreEquivalent(new[] { new Rect(1, 1, 1, 1) }, atlas.GetTaggedRects("Tag2"));

			atlas.SetTags("Tag1", new[] { 1 });
			CollectionAssert.IsEmpty(atlas.GetTaggedIndices("Tag2"));
			CollectionAssert.IsEmpty(atlas.GetTaggedRects("Tag2"));
		}
	}
}