/*
 * Circle.cs
 * RVO2 Library C#
 *
 * Copyright 2008 University of North Carolina at Chapel Hill
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Please send all bug reports to <geom@cs.unc.edu>.
 *
 * The authors may be contacted via:
 *
 * Jur van den Berg, Stephen J. Guy, Jamie Snape, Ming C. Lin, Dinesh Manocha
 * Dept. of Computer Science
 * 201 S. Columbia St.
 * Frederick P. Brooks, Jr. Computer Science Bldg.
 * Chapel Hill, N.C. 27599-3175
 * United States of America
 *
 * <http://gamma.cs.unc.edu/RVO2/>
 */

/*
 * Example file showing a demo with 250 agents initially positioned evenly
 * distributed on a circle attempting to move to the antipodal position on the
 * circle.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RVO
{
    class Circle : MonoBehaviour
    {
        /* Store the gameObjects of the agents */
        private IList<GameObject> agentObjs = new List<GameObject>();

        [Header("Variables")] public int agentNums = 250;
        
        [Header("Nodes")]
        public GameObject agentNode;

        public GameObject obstacleNode;

        public GameObject agentPrefab;

        /* Store the goals of the agents. */
        IList<Vector2> goals;

        Circle()
        {
            goals = new List<Vector2>();
        }
        
        // Start is called before the first frame update
        void Start()
        {
            // Blocks blocks = new Blocks();
            /* Set up the scenario. */
            setupScenario();

            if (agentNode == null)
                agentNode = GameObject.Find("Agents");

            if (obstacleNode == null)
                obstacleNode = GameObject.Find("Obstacles");

            if (agentNums <= 0)
            {
                agentNums = 1;
            }
        }

        // Update is called once per frame
        void Update()
        {
        
            /* Perform (and manipulate) the simulation. */
            if (!reachedGoal())
            {
                updateVisualization();
                setPreferredVelocities();
                Simulator.Instance.doStep();
            }
        }

        void setupScenario()
        {
            /* Specify the global time step of the simulation. */
            Simulator.Instance.setTimeStep(0.25f);

            /*
             * Specify the default parameters for agents that are subsequently
             * added.
             */
            Simulator.Instance.setAgentDefaults(15.0f, 10, 10.0f, 10.0f, 1.5f, 2.0f, new Vector2(0.0f, 0.0f));

            /*
             * Add agents, specifying their start position, and store their
             * goals on the opposite side of the environment.
             */
            for (int i = 0; i < agentNums; ++i)
            {
                addAgent(200.0f * new Vector2((float)Math.Cos(i * 2.0f * Math.PI / agentNums),
                             (float)Math.Sin(i * 2.0f * Math.PI / agentNums)));
                goals.Add(-Simulator.Instance.getAgentPosition(i));
            }
        }
        
        private void addAgent(Vector2 agentPos)
        {
            Simulator.Instance.addAgent(agentPos);

            // 下面为创建 Agent GameObjects
            if (agentPrefab == null)
                return;

            GameObject newAgentObj = GameObject.Instantiate(agentPrefab, agentNode.transform, true);
            newAgentObj.transform.position = new Vector3(agentPos.x(), 0, agentPos.y());
            agentObjs.Add(newAgentObj);
        }

        void updateVisualization()
        {
            for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i)
            {
                // Debug.LogWarningFormat(" {0}", Simulator.Instance.getAgentPosition(i));
                Vector2 pos = Simulator.Instance.getAgentPosition(i);
                Vector2 velocity = Simulator.Instance.getAgentVelocity(i);
                
                GameObject agentObj = agentObjs[i];
                if (agentObj != null)
                {
                    agentObj.transform.position = new Vector3(pos.x(), 0, pos.y());
                    Vector3 rota = new Vector3(velocity.x(), 0, velocity.y());
                    if (rota != Vector3.zero)
                        agentObj.transform.rotation = Quaternion.LookRotation(rota);
                }
            }
        }

        void setPreferredVelocities()
        {
            /*
             * Set the preferred velocity to be a vector of unit magnitude
             * (speed) in the direction of the goal.
             */
            for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i)
            {
                Vector2 goalVector = goals[i] - Simulator.Instance.getAgentPosition(i);

                if (RVOMath.absSq(goalVector) > 1.0f)
                {
                    goalVector = RVOMath.normalize(goalVector);
                }

                Simulator.Instance.setAgentPrefVelocity(i, goalVector);
            }
        }

        bool reachedGoal()
        {
            /* Check if all agents have reached their goals. */
            for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i)
            {
                if (RVOMath.absSq(Simulator.Instance.getAgentPosition(i) - goals[i]) > Simulator.Instance.getAgentRadius(i) * Simulator.Instance.getAgentRadius(i))
                {
                    return false;
                }
            }

            return true;
        }

        // public static void Main(string[] args)
        // {
        //     Circle circle = new Circle();
        //
        //     /* Set up the scenario. */
        //     circle.setupScenario();
        //
        //     /* Perform (and manipulate) the simulation. */
        //     do
        //     {
        //         #if RVO_OUTPUT_TIME_AND_POSITIONS
        //         circle.updateVisualization();
        //         #endif
        //         circle.setPreferredVelocities();
        //         Simulator.Instance.doStep();
        //     }
        //     while (!circle.reachedGoal());
        // }
    }
}
