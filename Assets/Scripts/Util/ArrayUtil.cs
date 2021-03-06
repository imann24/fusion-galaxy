﻿/*
 * Author(s): Isaiah Mann 
 * Description: Static class with array helper functions
 */

using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public static class ArrayUtil {

	public static T[] GetRange<T> (T[] source, int start, int length) {
		T[] range = new T[length];
		Array.Copy(source, start, range, 0, length);
		return range;
	}
		
	public static string [] RemoveEmptyElements (string [] original) {
		List<string> modified = new List<string>();

		for (int i = 0; i < original.Length; i++) {
			if (!string.IsNullOrEmpty(original[i]) && 
				original[i].Trim().Length != 0 &&
				original[i][0] != '\r' && 
				original[i][0] != '\n') {
				modified.Add(original[i]);
			}
		}

		return modified.ToArray();
	}

	public static T[] RemoveNullElements<T> (T[] original) {
		List<T> modified = new List<T>();
		for (int i = 0; i < original.Length; i++) {
			if (original[i] != null) {
				modified.Add(original[i]);
			}
		}
		return modified.ToArray();
	}

	public static T[] RemoveFirst<T> (T[] source) {
		T[] modified = new T[source.Length - 1];

		Array.Copy(
			source,
			1,
			modified,
			0,
			modified.Length);

		return modified;
	}

	public static T Pop<T> (ref T[] arrayToModify) {
		T firstElement = arrayToModify[0];
		arrayToModify = RemoveFirst(arrayToModify);
		return firstElement;
	}

	public static T[] Concat<T> (T[] source1, T[] source2) {
		T[] combined = new T[source1.Length + source2.Length];

		Array.Copy(source1, combined, source1.Length);
		Array.Copy(source2, 0, combined, source1.Length, source2.Length);

		return combined;
	}

	public static string ToString<T> (T[] source) {
		string arrayAsString = "";

		for (int i = 0; i < source.Length; i++) {
			arrayAsString += source[i] + ",\n";
		}

		return arrayAsString;
	}

	public static int IndexOf<T> (T[] source, T element) where T : IComparable {
		for (int i = 0; i < source.Length; i++) {
			if (source[i].CompareTo(element) == 0) {
				return i;
			}
		}

		throw new KeyNotFoundException();
	}

	// Overloaded method because GameObject does not implement IComparable
	public static int IndexOf (GameObject[] source, GameObject element) {
		for (int i = 0; i < source.Length; i++) {
			if (source[i] == element){ 
				return i;
			}
		}

		throw new KeyNotFoundException();
	}

	// Overloaded method because MonoBehaviour does not implement IComparable
	public static int MonoIndexOf<K> (K[] source, K element) where K : MonoBehaviour {
		for (int i = 0; i < source.Length; i++) {
			if (source[i].Equals(element)){ 
				return i;
			}
		}

		throw new KeyNotFoundException();
	}

	public static T Remove<T> (ref T[]source, T element) where T : IComparable {
		int index = IndexOf(
			source,
			element);

		T[] modified = new T[source.Length-1];

		Array.ConstrainedCopy (
			source,
			0,
			modified,
			0,
			index-1);

		Array.ConstrainedCopy (
			source,
			index + 1,
			modified,
			index,
			source.Length - index - 1
		);

		return element;
	}


	public static void Add<T> (ref T[]source, T element) {
		T[] modified = new T[source.Length+1];
		modified[source.Length] = element;
	}

	public static string ToString<T> (List<T>[] source) {
		string result = string.Empty;

		for (int i = 0; i < source.Length; i++) {
			result +=  (i+1) + ". {\n" + ToString(source[i].ToArray()) + "}\n"; 
		}

		return result;
	}

	public static bool Contains<T> (T[] source, T element) where T : IComparable {
		for (int i = 0; i < source.Length; i++) {
			if (source[i].CompareTo(element) == 0) {
				return true;
			}
		}

		return false;
	}

	public static void SetAll<T> (T[]source, T value) {
		for (int i = 0; i < source.Length; i++) {
			source[i] = value;
		}
	}

	public static T[] Concat<T> (params T[][] arrays) {
		T[] result = arrays[0];
		for (int i = 0; i < arrays.Length; i++) {
			result = result.Concat(arrays[i]).ToArray();
		}
		return result;
	}
}