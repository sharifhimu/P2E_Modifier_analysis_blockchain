using System;
using System.Collections.Generic;
using System.Linq;
using Nethereum.Util; // Keccak256

public class MerkleTree
{
    private List<byte[]> _leaves;
    private List<List<byte[]>> _levels;
    private static readonly Sha3Keccack Sha3 = new Sha3Keccack();

    public MerkleTree(List<byte[]> leaves)
    {
        if (leaves == null || leaves.Count == 0)
            throw new ArgumentException("Leaves cannot be null or empty");

        // _leaves = leaves.Select(l => Sha3(l)).ToList(); // hash leaves first
        _leaves = leaves; // don't hash its already hased
        _levels = new List<List<byte[]>>();
        BuildTree();
    }

    private void BuildTree()
    {
        _levels.Clear();
        _levels.Add(_leaves);

        var current = _leaves;

        while (current.Count > 1)
        {
            var next = new List<byte[]>();

            for (int i = 0; i < current.Count; i += 2)
            {
                if (i + 1 < current.Count)
                {
                    next.Add(HashPair(current[i], current[i + 1]));
                }
                else
                {
                    // duplicate last node if odd count
                    next.Add(HashPair(current[i], current[i]));
                }
            }

            _levels.Add(next);
            current = next;
        }
    }

    public byte[] GetRoot()
    {
        return _levels.Last().First();
    }

    public List<byte[]> GetProof(int index)
    {
        var proof = new List<byte[]>();
        int pos = index;

        for (int level = 0; level < _levels.Count - 1; level++)
        {
            var nodes = _levels[level];
            int sibling = (pos % 2 == 0) ? pos + 1 : pos - 1;

            if (sibling < nodes.Count)
                proof.Add(nodes[sibling]);

            pos /= 2;
        }

        return proof;
    }

    // ----------------- Helper functions -----------------

    private static byte[] HashPair(byte[] a, byte[] b)
    {
        // lexicographic compare so it matches OpenZeppelin _hashPair
        bool aFirst = false;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            if (a[i] < b[i]) { aFirst = true; break; }
            if (a[i] > b[i]) { aFirst = false; break; }
            if (i == Math.Min(a.Length, b.Length) - 1) aFirst = a.Length <= b.Length;
        }

        var left  = aFirst ? a : b;
        var right = aFirst ? b : a;

        var data = new byte[left.Length + right.Length];
        Buffer.BlockCopy(left, 0, data, 0, left.Length);
        Buffer.BlockCopy(right, 0, data, left.Length, right.Length);
        return Sha3.CalculateHash(data);
    }

}
