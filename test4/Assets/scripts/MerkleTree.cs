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

    private static byte[] HashPair(byte[] left, byte[] right)
    {
        // Compare lexicographically and sort
        int cmp = CompareBytes(left, right);
        var a = (cmp <= 0) ? left : right;
        var b = (cmp <= 0) ? right : left;

        var data = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, data, 0, a.Length);
        Buffer.BlockCopy(b, 0, data, a.Length, b.Length);
        return Sha3.CalculateHash(data);
    }

    private static int CompareBytes(byte[] a, byte[] b)
    {
        for (int i = 0; i < 32; i++)
        {
            int diff = a[i].CompareTo(b[i]);
            if (diff != 0) return diff;
        }
        return 0;
    }

}
