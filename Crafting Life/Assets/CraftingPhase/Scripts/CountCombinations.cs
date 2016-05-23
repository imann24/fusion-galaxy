using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class CountCombinations : MonoBehaviour {
	void Start(){
		if(GlobalVars.NUMBER_OF_COMBINATIONS.Count == 0){
			CountElementCombinations ();
		}
	}
	public void CountElementCombinations(){
		foreach(Combination combo in GlobalVars.RECIPES){
			if(GlobalVars.NUMBER_OF_COMBINATIONS.ContainsKey(combo.getFirstParent())){
				GlobalVars.NUMBER_OF_COMBINATIONS[combo.getFirstParent()]++;
			}
			else{
				GlobalVars.NUMBER_OF_COMBINATIONS[combo.getFirstParent()] = 1;
			}
			if(GlobalVars.NUMBER_OF_COMBINATIONS.ContainsKey(combo.getSecondParent())){
				GlobalVars.NUMBER_OF_COMBINATIONS[combo.getSecondParent()]++;
			}
			else{
				GlobalVars.NUMBER_OF_COMBINATIONS[combo.getSecondParent()] = 1;
			}
		}
	}
}
