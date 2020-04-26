using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Code will modify the animator to make sure the proper animations will occur
//Example: B combinator is created, must do animations for combinator B
public class AnimateCombinator : MonoBehaviour
{
    [SerializeField] Combinator[] combinatorList;
    [SerializeField] string[] triggerName;

    Animator comb_animator; // Var holding animator

    //Index that will track the combinator in combinator list and trigger name
    private int myCombInt;

    // When the objcect is created:
    void Awake()
    {
        //Get animator
        comb_animator = gameObject.GetComponent<Animator>();

        //Goes through list of combinators to see which combinator corresponds to this
        //current combinator object
        for (int i = 0; i < combinatorList.Length; i++)
        {
            if (combinatorList[i] == gameObject.GetComponent<DraggableSpell>().myCombinator)
            {   
                myCombInt = i;
            }
         }

    }

    //Fucntion that sets the string variable myTrigger_name to the right name
    public void UseCombinator()
    {
        //If animator is NULL do nothing!\
        //ADDED BECAUSE AFTER DESTROYED COMINATOR, THIS WAS STILL TRYING TO BE ACCESSED
        if (comb_animator != null)
        {
            comb_animator.SetTrigger(triggerName[myCombInt]);
        }
    }

}
